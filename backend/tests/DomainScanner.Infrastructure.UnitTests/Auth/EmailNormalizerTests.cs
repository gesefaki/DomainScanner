using DomainScanner.Infrastructure.Auth.Authentication.Normalization;
using FluentAssertions;

namespace DomainScanner.Infrastructure.UnitTests.Auth;

/// <summary>
/// Unit tests for <see cref="EmailNormalizer"/>
/// </summary>
public class EmailNormalizerTests
{
    private readonly EmailNormalizer _normalizer = new();

    [Theory]
    [InlineData("user@example.com", "user@example.com")]
    [InlineData("USER@EXAMPLE.COM", "user@example.com")]
    [InlineData(" User@Example.COM ", "user@example.com")]
    [InlineData("user+tag@example.com", "user+tag@example.com")]
    [InlineData("Use\u0301r@Example.COM", "usér@example.com")]
    public void Normalize_ReturnsExpectedValue(
        string source,
        string expected
    )
    {
        // Act
        var result = _normalizer.Normalize(source);
        
        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void Normalize_NullEmail_ThrowsArgumentNullException()
    {
        // Act
        var action = () => _normalizer.Normalize(null!);

        // Assert
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Normalize_AlreadyNormalizedValue_IsIdempotent()
    {
        // Arrange
        const string source = "User@Example.COM";

        // Act
        var first = _normalizer.Normalize(source);
        var second = _normalizer.Normalize(first);
        
        // Assert
        Assert.Equal(second, first);
    }
}