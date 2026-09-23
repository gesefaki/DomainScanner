using AutoMapper;
using DomainScanner.Application.Abstractions.Auth;
using DomainScanner.Application.Abstractions.Persistence;
using DomainScanner.Application.Abstractions.Persistence.Common;
using DomainScanner.Application.Handlers.Domains.Commands.CreateDomain;
using DomainScanner.Application.UnitTests.TestData.Domains;
using DomainScanner.Contracts.DTOs.Domains.Responses;
using DomainScanner.Contracts.Exceptions.Users;
using DomainScanner.Domain.Entities;
using FluentAssertions;
using Moq;
using System.Linq.Expressions;
using DomainScanner.Contracts.Options.Domains;
using DomainScanner.Contracts.Exceptions.Domains;
using Microsoft.Extensions.Options;

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
            _currentUser.Object,
            Options.Create(new DomainQuotaOptions { MaxDomainsPerUser = 2 }));
    }

    /// <summary>
    /// An active owner below quota can create a domain with monitoring enabled.
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public async Task Handle_CreateDomainWhenUserExists_CreatesItAndReturns(int ownedCount)
    {
        // Arrange
        var user = new User
        {
            Id = _fakeUserId,
            IsActive = true
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

        SetupUser(user);
        SetupDomainCount(ownedCount);

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
        var result = await _handler.Handle(
            command,
            CancellationToken.None
            );
        
        // Assert
        createdDomain.Should().NotBeNull();
        createdDomain.Address.Should().Be(FakeDomainAddress);
        createdDomain.UserId.Should().Be(_fakeUserId);
        createdDomain.MonitoringEnabled.Should().BeTrue();
        createdDomain.IsActive.Should().BeFalse();
        result.Should().BeSameAs(expectedResponse);

        _domainsRepository.Verify(x => x.CreateAsync(
                It.Is<DomainEntity>(d =>
                    d.Address == FakeDomainAddress),
                It.IsAny<CancellationToken>()),
            Times.Once);

    }
    
    /// <summary>
    /// A missing or inactive current user cannot create a domain, even if another active user exists.
    /// </summary>
    [Theory]
    [InlineData("missing")]
    [InlineData("inactive")]
    [InlineData("foreign")]
    public async Task Handle_WithoutActiveOwner_ThrowsAndDoesNotCreate(string scenario)
    {
        // Arrange
        var command = new DomainCommandBuilder()
            .WithId(_fakeDomainId)
            .WithAddress(FakeDomainAddress)
            .BuildCreateCommand();

        _currentUser.SetupGet(x => x.Id).Returns(_fakeUserId);
        SetupUser(scenario == "missing" ? null : new User
        {
            Id = scenario == "foreign" ? Guid.NewGuid() : _fakeUserId,
            IsActive = scenario != "inactive"
        });
        
        // Act
        var action = () => _handler.Handle(
            command,
            CancellationToken.None
        );

        // Assert
        await action.Should().ThrowAsync<UserNotFoundException>();
        _domainsRepository.Verify(x => x.CountAsync(
            It.IsAny<Expression<Func<DomainEntity, bool>>>(),
            It.IsAny<CancellationToken>()), Times.Never);

        _domainsRepository.Verify(x => x.CreateAsync(
                It.IsAny<DomainEntity>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

    }

    /// <summary>
    /// Reaching or exceeding the configured quota prevents insertion and mapping.
    /// Paused domains still consume quota; foreign domains never do.
    /// </summary>
    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    public async Task Handle_QuotaReached_DoesNotCreate(int ownedCount)
    {
        // Arrange
        _currentUser.SetupGet(x => x.Id).Returns(_fakeUserId);
        SetupUser(new User { Id = _fakeUserId, IsActive = true });
        SetupDomainCount(ownedCount);
        var command = new DomainCommandBuilder().BuildCreateCommand();
        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        // Act
        var error = await Record.ExceptionAsync(act);

        // Assert
        var quotaError = Assert.IsType<DomainQuotaExceededException>(error);
        Assert.Equal(2, quotaError.Limit);
        _domainsRepository.Verify(x => x.CreateAsync(
            It.IsAny<DomainEntity>(), It.IsAny<CancellationToken>()), Times.Never);
        _mapper.VerifyNoOtherCalls();
    }

    private void SetupUser(User? user)
    {
        _usersReadRepository.Setup(x => x.IsExistsByAttribute(
                It.IsAny<Expression<Func<User, bool>>>(), CancellationToken.None))
            .ReturnsAsync((Expression<Func<User, bool>> predicate, CancellationToken _) =>
                user is not null && predicate.Compile()(user));
    }

    private void SetupDomainCount(int count)
    {
        var domains = Enumerable.Range(0, count)
            .Select(_ => new DomainEntity
            {
                UserId = _fakeUserId, IsActive = false, MonitoringEnabled = false
            })
            .Append(new DomainEntity { UserId = Guid.NewGuid() }).ToArray();
        _domainsRepository.Setup(x => x.CountAsync(
                It.IsAny<Expression<Func<DomainEntity, bool>>>(), CancellationToken.None))
            .ReturnsAsync((Expression<Func<DomainEntity, bool>> predicate, CancellationToken _) =>
                domains.Count(predicate.Compile()));
    }
}
