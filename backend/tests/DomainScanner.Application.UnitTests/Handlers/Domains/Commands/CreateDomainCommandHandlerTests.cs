using AutoMapper;
using DomainScanner.Application.Abstractions.Auth;
using DomainScanner.Application.Abstractions.Persistence;
using DomainScanner.Application.Abstractions.Persistence.Common;
using DomainScanner.Application.Handlers.Domains.Commands.CreateDomain;
using DomainScanner.Application.UnitTests.TestData.Domains;
using DomainScanner.Application.UnitTests.TestData.Mocks;
using DomainScanner.Contracts.DTOs.Domains.Responses;
using DomainScanner.Contracts.Exceptions.Users;
using DomainScanner.Domain.Entities;
using FluentAssertions;
using Moq;

namespace DomainScanner.Application.UnitTests.Handlers.Domains.Commands;

/// <summary>
/// Unit tests for <see cref="CreateDomainCommandHandler"/>.
/// </summary>
public class CreateDomainCommandHandlerTests
{
    private readonly Mock<IReadRepository<User, Guid>> _usersReadRepository = new();
    private readonly Mock<IRepository<DomainEntity, Guid>> _domainsRepository = new();
    private readonly Mock<IMapper> _mapper = new();
    private readonly Mock<ICurrentUser> _currentUser = new();
    private readonly CreateDomainCommandHandler _handler;

    private readonly Guid _fakeDomainId = Guid.NewGuid();
    private readonly Guid _fakeUserId = Guid.NewGuid();
    private const string FakeDomainAddress = "https://example.com/";

    public CreateDomainCommandHandlerTests()
    {
        _handler = new CreateDomainCommandHandler(
            _usersReadRepository.Object,
            _domainsRepository.Object,
            _mapper.Object,
            _currentUser.Object);
    }

    /// <summary>
    /// Tests that a domain with existing user is successfully created.
    /// </summary>
    [Fact]
    public async Task Handle_CreateDomainWhenUserExists_CreatesItAndReturns()
    {
        // Arrange
        var user = new User
        {
            Id = _fakeUserId
        };
        _currentUser.SetupGet(x => x.Id).Returns(_fakeUserId);

        DomainEntity? createdDomain = null;

        var command = new DomainCommandBuilder()
            .WithId(_fakeDomainId)
            .WithAddress(FakeDomainAddress)
            .BuildCreateCommand();

        var expectedResponse = new DomainResponseBuilder()
            .WithId(_fakeDomainId)
            .WithAddress(FakeDomainAddress)
            .WithUserId(_fakeUserId)
            .Build();

        _usersReadRepository.SetupFindAsync(_fakeUserId, user);

        _domainsRepository
            .Setup(x => x.CreateAsync(
                It.IsAny<DomainEntity>(),
                It.IsAny<CancellationToken>()))
            .Callback<DomainEntity, CancellationToken>((entity, _) =>
                createdDomain = entity)
            .ReturnsAsync((DomainEntity entity, CancellationToken _) =>
            {
                entity.Id = _fakeDomainId;
                return entity;
            });

        _mapper
            .Setup(x => x.Map<DomainResponse>(It.IsAny<DomainEntity>()))
            .Returns(expectedResponse);
        
        // Act
        await _handler.Handle(
            command,
            CancellationToken.None
            );
        
        // Assert
        createdDomain.Should().NotBeNull();
        createdDomain.Address.Should().Be(FakeDomainAddress);
        createdDomain.UserId.Should().Be(_fakeUserId);

        _domainsRepository.Verify(x => x.CreateAsync(
                It.Is<DomainEntity>(d =>
                    d.Address == FakeDomainAddress),
                It.IsAny<CancellationToken>()),
            Times.Once);

    }
    
    /// <summary>
    /// Tests that creating a domain with non-existing user throws <see cref="UserNotFoundException"/>.
    /// </summary>
    [Fact]
    public async Task Handle_CreateDomainWhenUserDoesNotExists_ThrowAndDoesNotCreate()
    {
        // Arrange
        var command = new DomainCommandBuilder()
            .WithId(_fakeDomainId)
            .WithAddress(FakeDomainAddress)
            .BuildCreateCommand();

        _currentUser.SetupGet(x => x.Id).Returns(_fakeUserId);
        _usersReadRepository.SetupFindAsync(_fakeUserId, (User?)null);
        
        // Act
        var action = () => _handler.Handle(
            command,
            CancellationToken.None
        );

        // Assert
        await action.Should().ThrowAsync<UserNotFoundException>();

        _domainsRepository.Verify(x => x.CreateAsync(
                It.IsAny<DomainEntity>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

    }
}
