using AutoMapper;
using DomainScanner.Application.Abstractions.Persistence.Common;
using DomainScanner.Application.Handlers.Users.Queries.GetUserById;
using DomainScanner.Application.UnitTests.TestData.Mocks;
using DomainScanner.Application.UnitTests.TestData.Users;
using DomainScanner.Contracts.DTOs.Users.Responses;
using DomainScanner.Contracts.Exceptions.Users;
using DomainScanner.Domain.Entities;
using FluentAssertions;
using Moq;

namespace DomainScanner.Application.UnitTests.Handlers.Users.Queries;

/// <summary>
/// Unit tests for <see cref="GetUserByIdQueryHandler"/>.
/// </summary>
public class GetUserByIdQueryHandlerTests
{
    private readonly Mock<IReadRepository<User, Guid>> _repository = new();
    private readonly Mock<IMapper> _mapper = new();
    private readonly GetUserByIdQueryHandler _handler;

    public GetUserByIdQueryHandlerTests()
    {
        _handler = new GetUserByIdQueryHandler(
            _repository.Object,
            _mapper.Object);
    }

    /// <summary>Returns the mapped response when the user exists.</summary>
    [Fact]
    public async Task Handle_WhenUserExists_ReturnsMappedResponse()
    {
        // Arrange
        var user = new UserBuilder().Build();
        
        var response = UserResponseBuilder.Build(user);
        
        _repository.SetupFindAsync(user.Id, user);
        
        _mapper.Setup(x => x.Map<UserResponse>(user)).Returns(response);

        // Act
        var result = await _handler.Handle(new GetUserByIdQuery(user.Id), CancellationToken.None);

        // Assert
        result.Should().Be(response);
        _mapper.Verify(x => x.Map<UserResponse>(user), Times.Once);
    }

    /// <summary>Throws and does not map when the user cannot be found.</summary>
    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ThrowsUserNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repository.SetupFindAsync(id, (User?)null);

        // Act
        var action = () => _handler.Handle(new GetUserByIdQuery(id), CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<UserNotFoundException>();
        _mapper.Verify(x => x.Map<UserResponse>(It.IsAny<User>()), Times.Never);
    }
}
