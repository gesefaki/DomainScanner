using DomainScanner.Contracts.Helpers;
using DomainScanner.Domain.Entities;
using FluentAssertions;

namespace DomainScanner.Contracts.UnitTests.Helpers;

/// <summary>
/// Unit tests for the <see cref="DomainsHelper"/> class.
/// </summary>
public class DomainsHelperTests
{
    /// <summary>
    /// Tests that <see cref="DomainsHelper.AddressToUri"/> returns a valid <see cref="Uri"/>
    /// when provided with a well-formed absolute URL address.
    /// </summary>
    /// <param name="validAddress">
    /// A string representing a valid absolute URL (e.g., "https://example.com/").
    /// The address must include a scheme (http, https, ftp, etc.).
    /// </param>
    [Theory]
    [InlineData("https://example.com/")]
    [InlineData("http://example.com/")]
    [InlineData("https://sub.domain.com/path/")]
    public void AddressToUri_WhenValid_ReturnsUri(string validAddress)
    {
        // Arrange
        var domain = new DomainEntity
        {
            Address = validAddress
        };
        
        // Act
        var result = DomainsHelper.AddressToUri(domain);
        
        // Assert
        result.Should().NotBeNull();
        result.ToString().Should().Be(validAddress);
    }
    
    /// <summary>
    /// Tests that <see cref="DomainsHelper.AddressToUri"/> returns <c>null</c>
    /// when provided with malformed or invalid URL addresses.
    /// </summary>
    /// <param name="invalidAddress">
    /// A string that does not represent a valid absolute URL
    /// (e.g., missing scheme, malformed format).
    /// </param>
    [Theory]
    [InlineData("example.com/")]
    [InlineData("https:example.com/")]
    [InlineData("example")]
    public void AddressToUri_WhenInvalid_ReturnsNull(string invalidAddress)
    {
        // Arrange
        var domain = new DomainEntity
        {
            Address = invalidAddress
        };
        
        // Act
        var result = DomainsHelper.AddressToUri(domain);
        
        // Assert
        result.Should().BeNull();
    }
}