using DomainScanner.Contracts.Options.Auth;
using DomainScanner.Infrastructure.DI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DomainScanner.Infrastructure.UnitTests.Auth;

public class JwtOptionsValidationTests
{
    [Theory]
    [InlineData("short-secret", false)]
    [InlineData("12345678901234567890123456789012", true)]
    public void JwtSecret_RequiresAtLeast32Bytes(string secret, bool isValid)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JwtOptions:Issuer"] = "DomainScanner.Tests",
                ["JwtOptions:Audience"] = "DomainScanner.Api",
                ["JwtOptions:ExpiresHours"] = "1",
                ["JwtOptions:SecretKey"] = secret
            })
            .Build();

        var services = new ServiceCollection();
        services.AddUserAuthenticationInfrastructure(configuration);

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<JwtOptions>>();

        if (isValid)
        {
            Assert.Equal(secret, options.Value.SecretKey);
        }
        else
        {
            Assert.Throws<OptionsValidationException>(() => options.Value);
        }
    }
}
