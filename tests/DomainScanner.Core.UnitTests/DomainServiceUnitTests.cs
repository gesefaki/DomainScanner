using System.Net;
using System.Net.Http.Headers;
using System.Net.Security;
using DomainScanner.Core.Models;
using DomainScanner.Core.Interfaces;
using DomainScanner.Core.Services;
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
    
    [Fact]
    public async Task UpdateHealthAsync_ShouldUpdateDomain()
    {
        // Arrange
        var domain = new Domain { Id = 1, Name = "https://google.com", IsAvailable = true };
        var repo = new Mock<IDomainRepository>();
        repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(domain);
        
        var factory = new Mock<IHttpClientFabric>();
        var handler = new Mock<HttpMessageHandler>();

        handler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("test")
            });
        
        var client = new HttpClient(handler.Object);
        factory.Setup(f => f.CreateHttpClient()).Returns(client);
        
        var service = new DomainService(repo.Object, factory.Object);
        
        // Act
        var result = await service.UpdateHealthAsync(1);
        
        // Assert
        result.Should().NotBeNull();
        result.IsAvailable.Should().BeTrue();
        repo.Verify(r => r.UpdateAsync(It.Is<Domain>(d =>
            d.Id == 1 &&
            d.Name == "https://google.com" &&
            d.IsAvailable == true)), Times.Once);
    }

    [Fact]
    public async Task UpdateHealthAsync_ShouldNotUpdateDomain_WhenDomainNotFound()
    {
        // Arrange
        var repo = new Mock<IDomainRepository>();
        repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Domain?)null);
        
        var factory = new Mock<IHttpClientFabric>();
        factory.Setup(f => f.CreateHttpClient()).Returns(new HttpClient());
        
        var service = new DomainService(repo.Object, factory.Object);
        
        // Act
        var result = await service.UpdateHealthAsync(1);
        
        // Assert
        result.Should().BeNull();
    }
    
    [Fact]
    public async Task GetHealthAsync_ShouldReturnHealth()
    {
        // Arrange
        var domain = new Domain { Id = 1, Name = "https://google.com", IsAvailable = true };
        var repo = new Mock<IDomainRepository>();
        repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(domain);
        
        var factory = new Mock<IHttpClientFabric>();
        
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("test"),
            Headers = { Server = { new ProductInfoHeaderValue("nginx", "1.1") }}
        };
        response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        response.Content.Headers.ContentLength = 13;
        
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);
        
        var client = new HttpClient(handler.Object);
        var tls = new TlsCapture
        {
            SslPolicyErrors = SslPolicyErrors.None,
        };
        factory.Setup(f => f.CreateHttpClientNoRedirect()).Returns((client, tls));
        
        var service = new DomainService(repo.Object, factory.Object);
        
        // Act
        var result = await service.GetHealthAsync(1);
        
        // Assert
        
        // Main
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(200);
        result.IsSuccess.Should().BeTrue();
        result.ResponseTime.Should().BeGreaterThan(0);
        
        // Content
        result.ContentType.Should().Be("application/json");
        result.ContentLength.Should().Be(13);
        result.Server.Should().Be("nginx/1.1");
        
    }

    [Fact]
    public async Task GetHealthAsync_ShouldNotReturnHealth_WhenDomainNotFound()
    {
        // Arrange
        var repo =  new Mock<IDomainRepository>();
        repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Domain?)null);
        
        var factory = new Mock<IHttpClientFabric>();
        
        var service = new DomainService(repo.Object, factory.Object);
        
        // Act
        var result = await service.GetHealthAsync(1);
        
        // Assert
        result.Should().BeNull();
    }
}
