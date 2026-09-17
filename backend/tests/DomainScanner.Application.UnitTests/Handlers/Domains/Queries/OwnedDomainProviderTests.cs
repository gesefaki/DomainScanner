using System.Linq.Expressions;
using DomainScanner.Application.Abstractions.Auth;
using DomainScanner.Application.Abstractions.Persistence;
using DomainScanner.Application.Services.Auth;
using DomainScanner.Application.UnitTests.TestData.Domains;
using DomainScanner.Contracts.Exceptions.Common;
using DomainScanner.Contracts.Exceptions.Domains;
using DomainScanner.Domain.Entities;
using FluentAssertions;
using Moq;

namespace DomainScanner.Application.UnitTests.Handlers.Domains.Queries;

/// <summary>
/// Unit tests for <see cref="OwnedDomainProvider"/>.
/// </summary>
public class OwnedDomainProviderTests
{
    private readonly Mock<IRepository<DomainEntity, Guid>> _repository = new();
    private readonly Mock<ICurrentUser> _currentUser = new();
    private readonly OwnedDomainProvider _provider;

    public OwnedDomainProviderTests()
    {
        _provider = new OwnedDomainProvider(
            _repository.Object,
            _currentUser.Object);
    }

    /// <summary>
    /// The provider returns a domain only when its identifier and owner match the current user.
    /// </summary>
    [Fact]
    public async Task GetRequiredAsync_WhenDomainIsOwned_ReturnsDomain()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var ownedDomain = new DomainBuilder()
            .WithId(Guid.NewGuid())
            .WithUserId(userId)
            .Build();
        var foreignDomain = new DomainBuilder()
            .WithId(Guid.NewGuid())
            .WithUserId(Guid.NewGuid())
            .Build();
        DomainEntity[] domains = [ownedDomain, foreignDomain];

        _currentUser.SetupGet(x => x.IsAuthenticated).Returns(true);
        _currentUser.SetupGet(x => x.Id).Returns(userId);
        _repository
            .Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<DomainEntity, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((
                Expression<Func<DomainEntity, bool>> predicate,
                CancellationToken _) =>
                domains.SingleOrDefault(predicate.Compile()));

        // Act
        var result = await _provider.GetRequiredAsync(
            ownedDomain.Id,
            CancellationToken.None);

        // Assert
        result.Should().BeSameAs(ownedDomain);
    }

    /// <summary>
    /// A foreign domain is exposed as not found to avoid revealing its existence.
    /// </summary>
    [Fact]
    public async Task GetRequiredAsync_WhenDomainIsForeign_ThrowsDomainNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var foreignDomain = new DomainBuilder()
            .WithId(Guid.NewGuid())
            .WithUserId(Guid.NewGuid())
            .Build();

        _currentUser.SetupGet(x => x.IsAuthenticated).Returns(true);
        _currentUser.SetupGet(x => x.Id).Returns(userId);
        _repository
            .Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<DomainEntity, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((
                Expression<Func<DomainEntity, bool>> predicate,
                CancellationToken _) =>
                predicate.Compile()(foreignDomain)
                    ? foreignDomain
                    : null);

        // Act
        var action = () => _provider.GetRequiredAsync(
            foreignDomain.Id,
            CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<DomainNotFoundException>();
    }

    /// <summary>
    /// An anonymous request is rejected before the repository is queried.
    /// </summary>
    [Fact]
    public async Task GetRequiredAsync_WhenUserIsAnonymous_ThrowsWithoutQueryingRepository()
    {
        // Arrange
        _currentUser.SetupGet(x => x.IsAuthenticated).Returns(false);

        // Act
        var action = () => _provider.GetRequiredAsync(
            Guid.NewGuid(),
            CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<NonAuthenticatedException>();
        _repository.Verify(x => x.GetAsync(
            It.IsAny<Expression<Func<DomainEntity, bool>>>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }
}
