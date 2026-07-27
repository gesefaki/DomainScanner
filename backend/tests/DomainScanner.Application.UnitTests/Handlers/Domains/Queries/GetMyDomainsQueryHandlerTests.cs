using System.Linq.Expressions;
using AutoMapper;
using DomainScanner.Application.Abstractions.Auth;
using DomainScanner.Application.Abstractions.Persistence.Common;
using DomainScanner.Application.Handlers.Users.Queries.GetMyDomainsQuery;
using DomainScanner.Application.UnitTests.TestData.Domains;
using DomainScanner.Contracts.DTOs.Domains.Responses;
using DomainScanner.Domain.Entities;
using FluentAssertions;
using Moq;

namespace DomainScanner.Application.UnitTests.Handlers.Domains.Queries;

/// <summary>
/// Unit tests for <see cref="GetMyDomainsQueryHandler"/>.
/// </summary>
public class GetMyDomainsQueryHandlerTests
{
    private readonly Mock<IReadRepository<DomainEntity, Guid>> _repository = new();
    private readonly Mock<IMapper> _mapper = new();
    private readonly Mock<ICurrentUser> _currentUser = new();

    private readonly GetMyDomainsQueryHandler _handler;

    private readonly Guid _fakeUserId = Guid.NewGuid();
    private readonly Guid _anotherUserId = Guid.NewGuid();

    public GetMyDomainsQueryHandlerTests()
    {
        _handler = new GetMyDomainsQueryHandler(
            _repository.Object,
            _mapper.Object,
            _currentUser.Object);
    }

    /// <summary>
    /// Should return mapped domains when user has them and filter by UserId correctly.
    /// </summary>
    [Fact]
    public async Task Handle_WhenUserHasDomains_ReturnsMappedDomainsAndFiltersByUserId()
    {
        
        // Arrange
        var query = new GetMyDomainsQuery();

        _currentUser
            .SetupGet(x => x.Id)
            .Returns(_fakeUserId);
        
        var domains = new DomainBuilder()
            .BuildRange(2);

        var response = new DomainResponseBuilder()
            .Build(domains[0]);
        
        Expression<Func<DomainEntity, bool>>? capturedPredicate = null;

        _repository
            .Setup(x => x.GetAllWhereAsync(
                It.IsAny<Expression<Func<DomainEntity, bool>>>(),
                It.IsAny<CancellationToken>()))
            .Callback<Expression<Func<DomainEntity, bool>>, CancellationToken>(
                (predicate, _) => capturedPredicate = predicate)
            .ReturnsAsync([domains[0], domains[1]]);

        _mapper
            .Setup(x => x.Map<DomainResponse>(domains[0]))
            .Returns(response);
        _mapper
            .Setup(x => x.Map<DomainResponse>(domains[1]))
            .Returns(response);

        // Act
        var _ = (await _handler.Handle(
            query,
            CancellationToken.None)).ToList();
        
        // Assert
        capturedPredicate.Should().NotBeNull();
        capturedPredicate!.Compile()(new DomainEntity { UserId = _fakeUserId })
            .Should().BeTrue();
        capturedPredicate.Compile()(new DomainEntity { UserId = _anotherUserId })
            .Should().BeFalse();

        _repository.Verify(x => x.GetAllWhereAsync(
            It.IsAny<Expression<Func<DomainEntity, bool>>>(),
            It.IsAny<CancellationToken>()), Times.Once);
        _mapper.Verify(x => x.Map<DomainResponse>(domains[0]), Times.Once);
        _mapper.Verify(x => x.Map<DomainResponse>(domains[1]), Times.Once);
    }

    /// <summary>
    /// Should return empty collection when user has no domains.
    /// </summary>
    [Fact]
    public async Task Handle_WhenUserHasNoDomains_ReturnsEmptyCollection()
    {
        // Arrange
        var query = new GetMyDomainsQuery();

        _currentUser
            .SetupGet(x => x.Id)
            .Returns(_fakeUserId);
        
        _repository
            .Setup(x => x.GetAllWhereAsync(
                It.IsAny<Expression<Func<DomainEntity, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        
        // Act
        var result = (await _handler.Handle(
            query,
            CancellationToken.None)).ToList();
        
        // Assert
        result.Should().BeEmpty();
        _mapper.Verify(
            x => x.Map<DomainResponse>(It.IsAny<DomainEntity>()),
            Times.Never);
    }
}
