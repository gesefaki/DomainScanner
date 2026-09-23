using DomainScanner.Contracts.Options.Worker;
using DomainScanner.Worker.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DomainScanner.Api.IntegrationTests.Worker;

public sealed class WorkerOptionsTests
{
    [Fact]
    public void ConfigureWorker_BindsDomainChecksWorkerSection()
    {
        // Arrange
        using var services = Configure("MaxDomainsPerRun", "17");

        // Act
        var options = services.GetRequiredService<IOptions<DomainChecksWorkerOptions>>().Value;

        // Assert
        Assert.Equal(17, options.MaxDomainsPerRun);
        Assert.Equal(7, options.RetentionDays);
        Assert.Equal(10, options.MaxResultsPerDomain);
    }

    [Theory]
    [InlineData("BatchSize", "0")]
    [InlineData("MaxDomainsPerRun", "0")]
    [InlineData("MaxDomainsPerRun", "-1")]
    [InlineData("RetentionDays", "0")]
    [InlineData("RetentionDays", "-1")]
    [InlineData("MaxResultsPerDomain", "0")]
    [InlineData("MaxResultsPerDomain", "-1")]
    public void ConfigureWorker_RejectsInvalidLimits(string key, string value)
    {
        // Arrange
        using var services = Configure(key, value);
        Func<DomainChecksWorkerOptions> act = () =>
            services.GetRequiredService<IOptions<DomainChecksWorkerOptions>>().Value;

        // Act
        var error = Record.Exception(act);

        // Assert
        Assert.IsType<OptionsValidationException>(error);
    }

    private static ServiceProvider Configure(string key, string value)
    {
        // Deliberately use the deployed section name, not the production constant.
        var values = new Dictionary<string, string?>
        {
            ["DomainChecksWorker:RecurringJobId"] = "test-checks",
            ["DomainChecksWorker:CronExpression"] = "*/5 * * * *",
            ["DomainChecksWorker:QueueName"] = "domain-checks",
            ["DomainChecksWorker:BatchSize"] = "2",
            ["DomainChecksWorker:MaxDomainsPerRun"] = "3",
            ["DomainChecksWorker:RetentionDays"] = "7",
            ["DomainChecksWorker:MaxResultsPerDomain"] = "10",
            [$"DomainChecksWorker:{key}"] = value
        };
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(values).Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.ConfigureWorker(configuration);
        return services.BuildServiceProvider();
    }
}
