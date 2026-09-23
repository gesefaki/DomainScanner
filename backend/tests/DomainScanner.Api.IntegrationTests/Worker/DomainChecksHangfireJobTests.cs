using DomainScanner.Application.Abstractions.Persistence;
using DomainScanner.Application.Handlers.Domains.Commands.HttpSendAndSave;
using DomainScanner.Contracts.Options.Worker;
using DomainScanner.Domain.Entities;
using DomainScanner.Worker.Jobs;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace DomainScanner.Api.IntegrationTests.Worker;

/// <summary>Exercises job orchestration with real DI scopes and MediatR, without external services.</summary>
public sealed class DomainChecksHangfireJobTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    public async Task RunAsync_UsesBoundedSelection_SeparateScopes_AndPrunesHistory(int count)
    {
        // Arrange
        var state = new JobState { Ids = Enumerable.Range(0, count).Select(_ => Guid.NewGuid()).ToArray() };
        await using var services = CreateServices(state);
        using var cancellation = new CancellationTokenSource();
        var before = DateTime.UtcNow.AddDays(-7);

        // Act
        await CreateJob(services).RunAsync(cancellation.Token);

        // Assert
        Assert.Equal(3, state.SelectionLimit);
        Assert.Equal(state.Ids, state.CheckedIds);
        Assert.Equal(count, state.HandlerScopes.Distinct().Count());
        Assert.All(state.Tokens, token => Assert.Equal(cancellation.Token, token));
        Assert.Equal(1, state.CleanupCalls);
        Assert.Equal(10, state.RetainedCount);
        Assert.InRange(state.Cutoff, before, DateTime.UtcNow.AddDays(-7));
    }

    [Fact]
    public async Task RunAsync_ContinuesAfterIndividualFailure_AndPrunesHistory()
    {
        // Arrange
        var state = new JobState { Ids = [Guid.NewGuid(), Guid.NewGuid()], FailFirstCheck = true };
        await using var services = CreateServices(state);

        // Act
        await CreateJob(services).RunAsync(CancellationToken.None);

        // Assert
        Assert.Equal(state.Ids, state.CheckedIds);
        Assert.Equal(1, state.CleanupCalls);
    }

    [Fact]
    public async Task RunAsync_CancellationStopsChecks_AndSkipsCleanup()
    {
        // Arrange
        using var cancellation = new CancellationTokenSource();
        var state = new JobState { Ids = [Guid.NewGuid(), Guid.NewGuid()], CancelAfterCheck = cancellation };
        await using var services = CreateServices(state);
        Func<Task> act = () => CreateJob(services).RunAsync(cancellation.Token);

        // Act
        var error = await Record.ExceptionAsync(act);

        // Assert
        Assert.IsAssignableFrom<OperationCanceledException>(error);
        Assert.Equal(state.Ids.Take(1), state.CheckedIds);
        Assert.Equal(0, state.CleanupCalls);
    }

    [Fact]
    public async Task RunAsync_SelectionFailurePropagates_AndSkipsCleanup()
    {
        // Arrange
        var state = new JobState { FailSelection = true };
        await using var services = CreateServices(state);
        Func<Task> act = () => CreateJob(services).RunAsync(CancellationToken.None);

        // Act
        var error = await Record.ExceptionAsync(act);

        // Assert
        Assert.IsType<InvalidOperationException>(error);
        Assert.Empty(state.CheckedIds);
        Assert.Equal(0, state.CleanupCalls);
    }

    [Fact]
    public async Task RunAsync_CleanupFailurePropagates()
    {
        // Arrange
        var state = new JobState { FailCleanup = true };
        await using var services = CreateServices(state);
        Func<Task> act = () => CreateJob(services).RunAsync(CancellationToken.None);

        // Act
        var error = await Record.ExceptionAsync(act);

        // Assert
        Assert.IsType<InvalidOperationException>(error);
        Assert.Equal(1, state.CleanupCalls);
    }

    private static ServiceProvider CreateServices(JobState state)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(state);
        services.AddScoped<ScopeMarker>();
        services.AddScoped<IDomainScanRepository, ScanRepository>();
        services.AddScoped<IDomainCheckRetentionService, RetentionService>();
        services.AddMediatR(configuration =>
            configuration.RegisterServicesFromAssemblyContaining<CheckHandler>());
        return services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
    }

    private static DomainChecksHangfireJob CreateJob(ServiceProvider services) => new(
        NullLogger<DomainChecksHangfireJob>.Instance,
        Options.Create(new DomainChecksWorkerOptions
        {
            BatchSize = 2, MaxDomainsPerRun = 3, RetentionDays = 7, MaxResultsPerDomain = 10
        }),
        services.GetRequiredService<IServiceScopeFactory>());

    public sealed class JobState
    {
        public IReadOnlyList<Guid> Ids { get; init; } = [];
        public List<Guid> CheckedIds { get; } = [];
        public List<Guid> HandlerScopes { get; } = [];
        public List<CancellationToken> Tokens { get; } = [];
        public int SelectionLimit { get; set; }
        public int CleanupCalls { get; set; }
        public DateTime Cutoff { get; set; }
        public int RetainedCount { get; set; }
        public bool FailFirstCheck { get; init; }
        public bool FailSelection { get; init; }
        public bool FailCleanup { get; init; }
        public CancellationTokenSource? CancelAfterCheck { get; init; }
    }

    public sealed class ScopeMarker
    {
        public Guid Id { get; } = Guid.NewGuid();
    }

    public sealed class ScanRepository(JobState state) : IDomainScanRepository
    {
        public Task<IReadOnlyList<Guid>> GetMonitorableIdsAsync(int limit, CancellationToken ct)
        {
            state.SelectionLimit = limit;
            state.Tokens.Add(ct);
            ct.ThrowIfCancellationRequested();
            if (state.FailSelection) throw new InvalidOperationException("Selection failed.");
            return Task.FromResult(state.Ids);
        }

        public Task<DomainEntity?> GetForScanAsync(Guid domainId, CancellationToken ct) =>
            throw new NotSupportedException("The job must send a command, not load domain entities.");
    }

    public sealed class CheckHandler(JobState state, ScopeMarker scope)
        : IRequestHandler<HttpSendAndSaveCommand, DomainCheckResult>
    {
        public Task<DomainCheckResult> Handle(HttpSendAndSaveCommand request, CancellationToken ct)
        {
            state.CheckedIds.Add(request.Id);
            state.HandlerScopes.Add(scope.Id);
            state.Tokens.Add(ct);
            state.CancelAfterCheck?.Cancel();
            ct.ThrowIfCancellationRequested();
            if (state.FailFirstCheck && state.CheckedIds.Count == 1)
                throw new InvalidOperationException("Scan failed.");
            return Task.FromResult(new DomainCheckResult { DomainId = request.Id });
        }
    }

    public sealed class RetentionService(JobState state) : IDomainCheckRetentionService
    {
        public Task PruneAsync(DateTime deleteBefore, int maxResultsPerDomain, CancellationToken ct)
        {
            state.CleanupCalls++;
            state.Cutoff = deleteBefore;
            state.RetainedCount = maxResultsPerDomain;
            state.Tokens.Add(ct);
            if (state.FailCleanup) throw new InvalidOperationException("Cleanup failed.");
            return Task.CompletedTask;
        }
    }
}
