using AutoMapper;
using DomainScanner.Application.Abstractions.Persistence.Common;
using DomainScanner.Application.Handlers.Users.Queries.GetAllUsers;
using DomainScanner.Application.UnitTests.TestData.Users;
using DomainScanner.Contracts.DTOs.Users.Responses;
using DomainScanner.Domain.Entities;
using FluentAssertions;
using Moq;

namespace DomainScanner.Application.UnitTests.Handlers.Users.Queries;

/// <summary>
/// Unit tests for <see cref="GetAllUsersQueryHandler"/>.
/// </summary>
public class GetAllUsersQueryHandlerTests
{
    private readonly Mock<IReadRepository<User, Guid>> _repository = new();
    private readonly Mock<IMapper> _mapper = new();
    private readonly GetAllUsersQueryHandler _handler;

    public GetAllUsersQueryHandlerTests()
    {
        _handler = new GetAllUsersQueryHandler(
            _repository.Object,
            _mapper.Object);
    }

    /// <summary>Returns mapped users in the order supplied by the repository.</summary>
    [Fact]
    public async Task Handle_WhenUsersExist_ReturnsMappedUsers()
    {
        // Arrange
        var firstUser = new UserBuilder().WithUsername("first").Build();
        
        var secondUser = new UserBuilder().WithUsername("second").Build();
        
        var firstResponse = UserResponseBuilder.Build(firstUser);
        
        var secondResponse = UserResponseBuilder.Build(secondUser);
        
        _repository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync([firstUser, secondUser]);
        
        _mapper.Setup(x => x.Map<UserResponse>(firstUser)).Returns(firstResponse);
        
        _mapper.Setup(x => x.Map<UserResponse>(secondUser)).Returns(secondResponse);

        // Act
        var result = (await _handler.Handle(new GetAllUsersQuery(), CancellationToken.None)).ToList();

        // Assert
        result.Should().ContainInOrder(firstResponse, secondResponse);
        
        _repository.Verify(x => x.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        
        _mapper.Verify(x => x.Map<UserResponse>(firstUser), Times.Once);
        _mapper.Verify(x => x.Map<UserResponse>(secondUser), Times.Once);
    }

    /// <summary>Returns an empty collection and skips mapping when no users exist.</summary>
    [Fact]
    public async Task Handle_WhenNoUsersExist_ReturnsEmptyCollection()
    {
        // Arrange
        _repository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync([]);

        // Act
        var result = (await _handler.Handle(new GetAllUsersQuery(), CancellationToken.None)).ToList();

        // Assert
        result.Should().BeEmpty();
        _mapper.Verify(x => x.Map<UserResponse>(It.IsAny<User>()), Times.Never);
    }
}