using System.Net;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using DomainScanner.Core.Models;
using DomainScanner.Core.Interfaces;
using DomainScanner.Core.Services;
using Xunit;
using FluentAssertions;
using Moq;
using Moq.Protected;

namespace DomainScanner.Core.UnitTests;

public class DomainServiceUnitTests
{
    
    [Fact]
    public async Task GetAllAsync_ShouldReturnDomains()
    {
        // Arrange
        var repo = new Mock<IDomainRepository>();
        repo.Setup(r => r.GetAllAsync()).ReturnsAsync([
            new Domain { Id = 1, Name = "name", IsAvailable = null },
            new Domain { Id = 2, Name = "asd", IsAvailable = true },
            new Domain { Id = 3, Name = "", IsAvailable = false }
        ]);

        var fabric = new Mock<IHttpClientFabric>();
        var service = new DomainService(repo.Object, fabric.Object);
        
        // Act
        var domains = await service.GetAllAsync();
        
        // Assert
        var enumerable = domains as Domain[] ?? domains.ToArray();
        enumerable.Should().NotBeEmpty();
        enumerable.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmpty_WhenDomainsEmpty()
    {
        // Arrange
        var repo = new Mock<IDomainRepository>();
        repo.Setup(r => r.GetAllAsync()).ReturnsAsync([]);
        
        var fabric = new Mock<IHttpClientFabric>();
        var service = new DomainService(repo.Object, fabric.Object);
        
        // Act
        var domains = await service.GetAllAsync();
        
        // Assert
        domains.Should().BeEmpty();
    }
    
    [Fact]
    public async Task GetByIdAsync_ShouldReturnDomain()
    {
        // Arrange
        var repo = new Mock<IDomainRepository>();
        repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync
            (new Domain { Id = 1, Name = "name", IsAvailable = true });
        
        var fabric = new Mock<IHttpClientFabric>();
        var service = new DomainService(repo.Object, fabric.Object);
        
        // Act
        var domain = await service.GetByIdAsync(1);
        
        // Assert
        domain.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenDomainNotFound()
    {
        // Arrange
        var repo = new Mock<IDomainRepository>();
        repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Domain?)null);
        
        var fabric = new Mock<IHttpClientFabric>();
        var service = new DomainService(repo.Object, fabric.Object);
        
        // Act
        var domain = await service.GetByIdAsync(1);
        
        // Assert
        domain.Should().BeNull();
    }
    
    [Fact]
    public async Task IsExistsByIdAsync_ShouldReturnTrue()
    {
        // Arrange
        var repo = new Mock<IDomainRepository>();
        repo.Setup(r => r.IsExistsByIdAsync(1)).ReturnsAsync(true);
        
        var fabric = new Mock<IHttpClientFabric>();
        var service = new DomainService(repo.Object, fabric.Object);
        
        // Act
        var exists = await service.IsExistsByIdAsync(1);
        
        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task IsExistsByIdAsync_ShouldReturnFalse_WhenDomainNotFound()
    {
        // Arrange
        var repo = new Mock<IDomainRepository>();
        repo.Setup(r => r.IsExistsByIdAsync(1)).ReturnsAsync(false);
        
        var fabric = new Mock<IHttpClientFabric>();
        var service = new DomainService(repo.Object, fabric.Object);
        
        // Act
        var exists = await service.IsExistsByIdAsync(1);
        
        // Assert
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task AddAsync_ShouldAddDomain()
    {
        // Arrange
        var repo = new Mock<IDomainRepository>();
        var fabric = new Mock<IHttpClientFabric>();
        var service = new DomainService(repo.Object, fabric.Object);
        
        // Act
        await service.AddAsync(new Domain { Id = 1, Name = "name", IsAvailable = true });
        
        // Assert
        repo.Verify(r => r.AddAsync(It.IsAny<Domain>()), Times.Once);
    }
    
    [Fact]
    public async Task RemoveAsync_ShouldRemoveDomain_Once()
    {
        // Arrange
        var repo = new Mock<IDomainRepository>();
        repo.Setup(r => r.RemoveAsync(1)).Returns(Task.CompletedTask);
        
        var fabric = new Mock<IHttpClientFabric>();
        var service = new DomainService(repo.Object, fabric.Object);
        
        // Act
        await service.RemoveAsync(1);
        
        // Assert
        repo.Verify(r => r.RemoveAsync(1), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateDomain()
    {
        // Arrange
        var repo = new Mock<IDomainRepository>();
        repo.Setup(r => r.IsExistsByIdAsync(1)).ReturnsAsync(true);
        
        var fabric = new Mock<IHttpClientFabric>();
        var service = new DomainService(repo.Object, fabric.Object);
        
        // Act
        var domain = new Domain { Id = 1, Name = "name", IsAvailable = true };
        await service.UpdateAsync(1, domain);
        
        repo.Verify(r => r.UpdateAsync(domain), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldNotUpdateDomain_WhenDomainNotFound()
    {
        // Arrange
        var repo = new Mock<IDomainRepository>();
        repo.Setup(r => r.IsExistsByIdAsync(1)).ReturnsAsync(false);
        
        var fabric = new Mock<IHttpClientFabric>();
        var service = new DomainService(repo.Object, fabric.Object);
        
        // Act
        var domain = new Domain { Id = 1, Name = "name", IsAvailable = true };
        await service.UpdateAsync(1, domain);
        
        // Assert
        repo.Verify(r => r.UpdateAsync(domain), Times.Never);
    }
}
