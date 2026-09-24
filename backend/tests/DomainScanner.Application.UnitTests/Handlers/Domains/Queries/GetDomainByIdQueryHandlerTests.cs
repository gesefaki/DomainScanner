using System.Linq.Expressions;
using DomainScanner.Application.Abstractions.Auth;
using DomainScanner.Application.Abstractions.Persistence.Common;
using DomainScanner.Application.Handlers.Domains.Queries.GetDomainById;
using DomainScanner.Contracts.Exceptions.Domains;
using DomainScanner.Domain.Entities;
using Moq;

namespace DomainScanner.Application.UnitTests.Handlers.Domains.Queries;

/// <summary>Checks domain ownership and selection of the latest check summary.</summary>
public sealed class GetDomainByIdQueryHandlerTests
{
    [Fact]
    public async Task OwnedDomain_ReturnsLatestCheckWithoutHistory()
    {
        var domain = new DomainEntity
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Address = "https://example.com",
            MonitoringEnabled = false
        };
        var time = DateTime.UtcNow;
        var older = new DomainCheckResult
        {
            Id = Guid.NewGuid(), DomainId = domain.Id,
            Kind = "http", Outcome = "down", CreatedAt = time.AddMinutes(-1)
        };
        var latest = new DomainCheckResult
        {
            Id = Guid.NewGuid(), DomainId = domain.Id,
            Kind = "http", Outcome = "up", CreatedAt = time
        };
        var owned = new Mock<IOwnedDomainProvider>();
        owned.Setup(provider => provider.GetRequiredAsync(domain.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(domain);
        var checks = new Mock<IReadRepository<DomainCheckResult, Guid>>();
        checks.Setup(repository => repository.GetAllWhereAsync(
                It.IsAny<Expression<Func<DomainCheckResult, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([older, latest]);
        var handler = new GetDomainByIdQueryHandler(owned.Object, checks.Object);

        var response = await handler.Handle(new GetDomainByIdQuery(domain.Id), CancellationToken.None);

        Assert.Equal(domain.Id, response.Id);
        Assert.False(response.MonitoringEnabled);
        Assert.Equal(latest.Id, response.LastCheck?.Id);
        Assert.Equal("up", response.LastCheck?.Outcome);
    }

    [Fact]
    public async Task DomainWithoutChecks_HasNullLastCheck()
    {
        var domain = new DomainEntity { Id = Guid.NewGuid() };
        var owned = new Mock<IOwnedDomainProvider>();
        owned.Setup(provider => provider.GetRequiredAsync(domain.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(domain);
        var checks = new Mock<IReadRepository<DomainCheckResult, Guid>>();
        checks.Setup(repository => repository.GetAllWhereAsync(
                It.IsAny<Expression<Func<DomainCheckResult, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var response = await new GetDomainByIdQueryHandler(owned.Object, checks.Object)
            .Handle(new GetDomainByIdQuery(domain.Id), CancellationToken.None);

        Assert.Null(response.LastCheck);
    }

    [Fact]
    public async Task ForeignDomain_DoesNotLoadChecks()
    {
        var id = Guid.NewGuid();
        var owned = new Mock<IOwnedDomainProvider>();
        owned.Setup(provider => provider.GetRequiredAsync(id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DomainNotFoundException(id));
        var checks = new Mock<IReadRepository<DomainCheckResult, Guid>>();

        await Assert.ThrowsAsync<DomainNotFoundException>(() =>
            new GetDomainByIdQueryHandler(owned.Object, checks.Object)
                .Handle(new GetDomainByIdQuery(id), CancellationToken.None));
        checks.VerifyNoOtherCalls();
    }
}
