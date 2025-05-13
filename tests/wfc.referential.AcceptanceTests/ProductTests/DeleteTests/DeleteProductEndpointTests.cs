using System.Net;
using System.Net.Http.Json;
using BuildingBlocks.Application.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.ProductAggregate;
using wfc.referential.Domain.ServiceAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.ProductTests.DeleteTests;

public class DeleteProductEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IProductRepository> _repoMock = new();

    public DeleteProductEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        // Clone the factory and customize the host
        var customisedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                // 🧹 Remove concrete registrations that hit the DB / Redis
                services.RemoveAll<IProductRepository>();
                services.RemoveAll<ICacheService>();

                // 🔌 Plug mocks back in
                services.AddSingleton(_repoMock.Object);
                services.AddSingleton(cacheMock.Object);
            });
        });

        _client = customisedFactory.CreateClient();
    }

    [Fact(DisplayName = "DELETE /api/products/{id} returns true when Product is deleted successfully")]
    public async Task Delete_ShouldReturnTrue_WhenProductExistsAndHasNoCities()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = Product.Create(
            ProductId.Of(productId),
            "001",
            "CashExpressTest",
            true
            );

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<Guid>(id => id == productId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _repoMock.Setup(r => r.GetServicesByProductIdAsync(It.Is<Guid>(id => id == productId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Service>()); // No service

        // Act
        var response = await _client.DeleteAsync($"/api/products/{productId}");
        var result = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeTrue();

        _repoMock.Verify(r => r.UpdateProductAsync(It.Is<Product>(r => r.Id == ProductId.Of(productId) && !r.IsEnabled.Equals(true)), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "DELETE /api/products/{id} returns 404 when Product does not exist")]
    public async Task Delete_ShouldReturn404_WhenProductDoesNotExist()
    {
        // Arrange
        var productId = Guid.NewGuid();
        _repoMock.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product)null); // Product not found

        // Act
        var response = await _client.DeleteAsync($"/api/products/{productId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

}
