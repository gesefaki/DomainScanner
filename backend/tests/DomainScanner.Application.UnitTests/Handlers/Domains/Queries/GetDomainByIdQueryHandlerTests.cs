using AutoMapper;
using DomainScanner.Application.Abstractions.Persistence.Common;
using DomainScanner.Application.Handlers.Domains.Queries.GetDomainById;
using DomainScanner.Application.UnitTests.TestData.Domains;
using DomainScanner.Application.UnitTests.TestData.Mocks;
using DomainScanner.Contracts.DTOs.Domains.Responses;
using DomainScanner.Contracts.Exceptions.Domains;
using DomainScanner.Domain.Entities;
using FluentAssertions;
using Moq;

namespace DomainScanner.Application.UnitTests.Handlers.Domains.Queries;

/// <summary>
/// Tests for <see cref="GetDomainByIdQueryHandler"/>.
/// </summary>
public class GetDomainByIdQueryHandlerTests
{
    private readonly Mock<IReadRepository<DomainEntity, Guid>> _repository = new();
    private readonly Mock<IMapper> _mapper = new();

    private readonly GetDomainByIdQueryHandler _handler;
    
    private readonly Guid _domainFakeId = Guid.NewGuid();
    private const string FakeDomainAddress = "https://example.com/";

    public GetDomainByIdQueryHandlerTests()
    {
        _handler = new GetDomainByIdQueryHandler(
            _repository.Object,
            _mapper.Object
        );
    }

    /// <summary>
    /// Should return mapped domain response when domain exists.
    /// </summary>
    [Fact]
    public async Task Handle_WhenDomainExists_ReturnsMappedResponse()
    {
        // Arrange
        var query = new GetDomainByIdQuery(_domainFakeId);

        var domain = new DomainBuilder()
            .WithId(_domainFakeId)
            .WithAddress(FakeDomainAddress)
            .Build();

        var expectedResponse = new DomainResponseBuilder().Build(domain);

        _repository
            .Setup(x => x.FindAsync(
                _domainFakeId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(domain);

        _mapper
            .Setup(x => x.Map<DomainResponse>(domain))
            .Returns(expectedResponse);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResponse);

        _repository.Verify(x => x.FindAsync(
                _domainFakeId,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _mapper.Verify(x => x.Map<DomainResponse>(domain), Times.Once);
    }

    /// <summary>
    /// Should throw <see cref="DomainNotFoundException"/> when domain not found.
    /// </summary>
    [Fact]
    public async Task Handle_WhenDomainDoesNotExists_Throw()
    {
        // Arrange
        var query = new GetDomainByIdQuery(_domainFakeId);

        _repository.SetupFindAsync(_domainFakeId, (DomainEntity?)null);
        
        // Act
        var action = () => _handler.Handle(
            query,
            CancellationToken.None);
        
        // Assert
        await action.Should().ThrowAsync<DomainNotFoundException>();
        
        _mapper.Verify(x => x.Map<DomainResponse>(It.IsAny<DomainEntity>()),
            Times.Never);

    }
}