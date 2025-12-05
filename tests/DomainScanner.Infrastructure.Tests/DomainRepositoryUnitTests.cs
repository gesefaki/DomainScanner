using DomainScanner.Infrastructure.Repository;
using DomainScanner.Infrastructure.Data;
using DomainScanner.Core.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DomainScanner.Infrastructure.Tests;

public class DomainRepositoryUnitTests
{
    private readonly DbContextOptions<ScannerDbContext> _options;
    
    public DomainRepositoryUnitTests()
    {
        _options = new DbContextOptionsBuilder<ScannerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllDomains()
    {
        // Arrange
        var domain1 = new Domain { Id = 1, Name = "Domain 1" };
        var domain2 = new Domain { Id = 2, Name = "Domain 2" };
        
        await using var context = new ScannerDbContext(_options);
        await context.Domains.AddAsync(domain1);
        await context.Domains.AddAsync(domain2);
        await context.SaveChangesAsync();
        
        var repository = new DomainRepository(context);
        
        // Act
        var result = await repository.GetAllAsync();
        
        // Assert
        IEnumerable<Domain> enumerable = result.ToList();
        enumerable.Should().NotBeNullOrEmpty();
        enumerable.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmptyIEnumerable_WhenIsEmpty()
    {
        // Arrange
        await using var context = new ScannerDbContext(_options);
        var repository = new DomainRepository(context);
        
        // Act
        var result = await repository.GetAllAsync();
        
        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnDomain()
    {
        // Arrange
        var domain1 = new Domain { Id = 1, Name = "Domain 1" };
        
        await using var context = new ScannerDbContext(_options);
        await context.Domains.AddAsync(domain1);
        await context.SaveChangesAsync();
        
        var repository = new DomainRepository(context);
        
        // Act
        var result = await repository.GetByIdAsync(1);
        
        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Name.Should().Be("Domain 1");
        result.IsAvailable.Should().Be(null);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenIsNull()
    {
        // Arrange
        await using var context = new ScannerDbContext(_options);
        var repository = new DomainRepository(context);
        
        // Act
        var result = await repository.GetByIdAsync(1);
        
        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task IsExistsByIdAsync_ShouldReturnTrue_WhenIsExists()
    {
        // Arrange
        var domain1 = new Domain { Id = 1, Name = "Domain 1" };
        
        await using var context = new ScannerDbContext(_options);
        await context.Domains.AddAsync(domain1);
        await context.SaveChangesAsync();
        
        var repository = new DomainRepository(context);
        
        // Act
        var result = await repository.IsExistsByIdAsync(1);
        
        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsExistsByIdAsync_ShouldReturnFalse_WhenIsNotFound()
    {
        // Arrange
        await using var context = new ScannerDbContext(_options);
        var repository = new DomainRepository(context);
        
        // Act
        var result = await repository.IsExistsByIdAsync(1);
        
        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task AddAsync_ShouldAddDomain()
    {
        // Arrange
        var domain1 = new Domain { Id = 1, Name = "Domain 1" };
        
        await using var context = new ScannerDbContext(_options);
        
        var repository = new DomainRepository(context);
        
        // Act
        await repository.AddAsync(domain1);
        
        // Assert
        context.Domains.Should().HaveCount(1);
    }
    
}
