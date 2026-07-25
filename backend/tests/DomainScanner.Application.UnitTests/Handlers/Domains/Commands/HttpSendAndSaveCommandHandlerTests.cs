using DomainScanner.Application.Abstractions.Persistence;
using DomainScanner.Application.Abstractions.Persistence.Common;
using DomainScanner.Application.Abstractions.Scanners;
using DomainScanner.Application.Handlers.Domains.Commands.HttpSendAndSave;
using DomainScanner.Application.UnitTests.TestData.Domains;
using DomainScanner.Application.UnitTests.TestData.Mocks;
using DomainScanner.Contracts.Exceptions.Domains;
using DomainScanner.Contracts.Helpers;
using DomainScanner.Domain.Entities;
using DomainScanner.Domain.Models;
using FluentAssertions;
using Moq;

namespace DomainScanner.Application.UnitTests.Handlers.Domains.Commands;

/// <summary>
/// Unit tests for <see cref="HttpSendAndSaveCommandHandler"/>.
/// </summary>
public class HttpSendAndSaveCommandHandlerTests
{
    private readonly Mock<IRepository<DomainEntity, Guid>> _domainsRepository = new();
    private readonly Mock<IWriteRepository<DomainCheckResult, Guid>> _checksWriteRepository = new();
    private readonly Mock<IHttpScanner> _http = new();

    private readonly HttpSendAndSaveCommandHandler _handler;

    private readonly Guid _fakeDomainId = Guid.NewGuid();
    private const string FakeDomainAddress = "https://example.com/";

    public HttpSendAndSaveCommandHandlerTests()
    {
        _handler = new HttpSendAndSaveCommandHandler(
            _domainsRepository.Object,
            _checksWriteRepository.Object,
            _http.Object
        );
    }

    /// <summary>
    /// Should send HTTP request, save check result and update domain when domain exists.
    /// </summary>
    [Fact]
    public async Task Handle_WhenDomainIsExists_SendHttpAndSaveAndReturnsResponse()
    {
        // Arrange

        // Primitives
        var command = new HttpSendAndSaveCommand(_fakeDomainId);

        var domain = new DomainBuilder()
            .WithId(_fakeDomainId)
            .WithAddress(FakeDomainAddress)
            .Inactive()
            .Build();

        var uri = DomainsHelper.AddressToUri(domain);

        var expectedHttpResponse = new HttpResponseObject
        {
            Address = FakeDomainAddress,
            StatusCode = 200,
            IsSuccess = true
        };

        DomainCheckResult? checkResult = null;

        // Repositories

        _domainsRepository.SetupFindAsync(_fakeDomainId, domain);

        _domainsRepository
            .Setup(x => x.Update(domain))
            .Returns(domain);

        _checksWriteRepository
            .Setup(x => x.CreateAsync(It.IsAny<DomainCheckResult>(), It.IsAny<CancellationToken>()))
            .Callback<DomainCheckResult, CancellationToken>((check, _) => checkResult = check)
            .ReturnsAsync((DomainCheckResult check, CancellationToken _) => check);

        // HTTP
        _http
            .Setup(x => x.GetHttpResponseAsync(uri!, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedHttpResponse);

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None
        );

        // Assert
        result.Should().NotBeNull();
        result.Address.Should().Be(FakeDomainAddress);
        result.StatusCode.Should().Be(200);
        
        checkResult!.Address.Should().Be(FakeDomainAddress);
        checkResult.StatusCode.Should().Be(200);
        checkResult.IsActive.Should().BeTrue();

        _checksWriteRepository.Verify(x => x.CreateAsync(It.Is<DomainCheckResult>(cr =>
                cr.Address == FakeDomainAddress &&
                cr.StatusCode == 200 &&
                cr.IsActive == true &&
                cr.CreatedAt > DateTime.MinValue),
            It.IsAny<CancellationToken>())
        );

        _domainsRepository.Verify(x => x.Update(
                It.Is<DomainEntity>(d =>
                    d.Id == _fakeDomainId &&
                    d.IsActive == true &&
                    d.UpdatedAt > DateTime.MinValue)),
            Times.Once);

        _http.Verify(x => x.GetHttpResponseAsync(It.IsAny<Uri>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
    
    /// <summary>
    /// Should throw <see cref="DomainNotFoundException"/> when domain is not found.
    /// </summary>
    [Fact]
    public async Task Handle_WhenDomainDoesNotExists_ThrowAndDoNothing()
    {
        // Arrange
        var command = new HttpSendAndSaveCommand(_fakeDomainId);
        
        _domainsRepository.SetupFindAsync(_fakeDomainId, (DomainEntity?)null);
        
        var action = () => _handler.Handle(
            command,
            CancellationToken.None
        );
        
        // Act + Assert
        await action.Should().ThrowAsync<DomainNotFoundException>();
        
        _http.Verify(x => x.GetHttpResponseAsync(
            It.IsAny<Uri>(),
            It.IsAny<CancellationToken>()),
            Times.Never);
        
        _checksWriteRepository.Verify(x => x.CreateAsync(
            It.IsAny<DomainCheckResult>(),
            It.IsAny<CancellationToken>()),
            Times.Never);
        
        _domainsRepository.Verify(x => x.Update(
            It.IsAny<DomainEntity>()),
            Times.Never);
    }
}