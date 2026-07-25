using AutoMapper;
using DomainScanner.Application.Abstractions.Persistence.Common;
using DomainScanner.Application.Handlers.Domains.Queries.GetAllDomains;
using DomainScanner.Application.UnitTests.TestData.Domains;
using DomainScanner.Contracts.DTOs.Domains.Responses;
using DomainScanner.Domain.Entities;
using FluentAssertions;
using Moq;

namespace DomainScanner.Application.UnitTests.Handlers.Domains.Queries;

/// <summary>
/// Unit tests for <see cref="GetAllDomainsQueryHandler"/>.
/// </summary>
public class GetAllDomainsQueryHandlerTests
{
    private readonly Mock<IReadRepository<DomainEntity, Guid>> _repository = new();
    private readonly Mock<IMapper> _mapper = new();

    private readonly GetAllDomainsQueryHandler _handler;

    public GetAllDomainsQueryHandlerTests()
    {
        _handler = new GetAllDomainsQueryHandler(
            _repository.Object,
            _mapper.Object
        );
    }

    /// <summary>
    /// Should return all mapped domains when they exist.
    /// </summary>
    [Fact]
    public async Task Handle_GetAllDomains_ReturnsAllDomains()
    {
        // Arrange
        var query = new GetAllDomainsQuery();
        
        var domains = new DomainBuilder()
            .BuildRange(2);

        var response = new DomainResponseBuilder().Build(domains[0]);

        _repository
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
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
        _repository.Verify(
            x => x.GetAllAsync(It.IsAny<CancellationToken>()),
            Times.Once);
        
        _mapper.Verify(x => x.Map<DomainResponse>(domains[0]), Times.Once);
        _mapper.Verify(x => x.Map<DomainResponse>(domains[1]), Times.Once);
    }

    /// <summary>
    /// Should return empty collection when no domains exist.
    /// </summary>
    [Fact]
    public async Task Handle_WhenNoDomainsExists_ReturnsEmptyCollection()
    {
        // Arrange
        var query = new GetAllDomainsQuery();
        
        _repository
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        
        // Act
        var result = (await _handler.Handle(
            query,
            CancellationToken.None
        )).ToList();

        // Assert
        result.Should().BeEmpty();

        _mapper.Verify(
            x => x.Map<DomainResponse>(It.IsAny<DomainEntity>()),
            Times.Never);
    }
}