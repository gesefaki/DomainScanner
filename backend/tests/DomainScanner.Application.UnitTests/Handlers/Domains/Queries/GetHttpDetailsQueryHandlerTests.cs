using DomainScanner.Application.Abstractions.Persistence.Common;
using DomainScanner.Application.Abstractions.Scanners;
using DomainScanner.Application.Handlers.Domains.Queries.GetHttpDetails;
using DomainScanner.Application.UnitTests.TestData.Domains;
using DomainScanner.Application.UnitTests.TestData.Mocks;
using DomainScanner.Contracts.Exceptions.Domains;
using DomainScanner.Domain.Entities;
using DomainScanner.Domain.Models;
using FluentAssertions;
using Moq;

namespace DomainScanner.Application.UnitTests.Handlers.Domains.Queries;

public class GetHttpDetailsQueryHandlerTests
{
    private readonly Mock<IReadRepository<DomainEntity, Guid>> _repository = new();
    private readonly Mock<IHttpScanner> _http = new();

    private readonly GetHttpDetailsQueryHandler _handler;

    private readonly Guid _fakeDomainId = Guid.NewGuid();
    private const string FakeDomainAddress = "https://example.com/";

    public GetHttpDetailsQueryHandlerTests()
    {
        _handler = new GetHttpDetailsQueryHandler(
            _repository.Object,
            _http.Object
            );
    }

    [Fact]
    public async Task Handle_WhenDomainExists_ReturnsHttpResponseDetails()
    {
        // Arrange
        var query = new GetHttpDetailsQuery(_fakeDomainId);
        
        var domain = new DomainBuilder()
            .WithId(_fakeDomainId)
            .WithAddress(FakeDomainAddress)
            .Build();
        
        var expectedResponse = new HttpResponseDetails
        {
            Address = FakeDomainAddress,
            StatusCode = 200,
            IsSuccess = true
        };

        _repository.SetupFindAsync(_fakeDomainId, domain);
        _http
            .Setup(x => x.GetHttpWithDetailsAsync(
                It.Is<Uri>(uri => uri.AbsoluteUri == FakeDomainAddress),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeSameAs(expectedResponse);
        _http.Verify(x => x.GetHttpWithDetailsAsync(
            It.Is<Uri>(uri => uri.AbsoluteUri == FakeDomainAddress),
            It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task Handle_WhenDomainDoesNotExists_ShouldThrow()
    {
        // Arrange
        var query = new GetHttpDetailsQuery(_fakeDomainId);

        _repository.SetupFindAsync(_fakeDomainId, (DomainEntity?)null);

        // Act + Assert
        var action = () => _handler.Handle(
            query,
            CancellationToken.None
        );

        await action.Should().ThrowAsync<DomainNotFoundException>();

        _http.Verify(x => x.GetHttpWithDetailsAsync(
                It.IsAny<Uri>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
