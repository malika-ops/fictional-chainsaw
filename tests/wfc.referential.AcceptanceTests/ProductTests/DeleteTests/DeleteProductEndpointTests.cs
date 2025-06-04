using BuildingBlocks.Application.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.ProductAggregate;
using wfc.referential.Domain.ServiceAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.ProductTests.DeleteTests;

public class DeleteProductEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IProductRepository> _repoMock = new();
    private readonly Mock<IServiceRepository> _repoServiceMock = new();

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
                services.RemoveAll<IServiceRepository>();
                services.RemoveAll<ICacheService>();

                // 🔌 Plug mocks back in
                services.AddSingleton(_repoMock.Object);
                services.AddSingleton(_repoServiceMock.Object);
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

        _repoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _repoServiceMock.Setup(r => r.GetByConditionAsync(It.IsAny<Expression<Func<Service, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Service>()); // No service

        // Act
        var response = await _client.DeleteAsync($"/api/products/{productId}");
        var result = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeTrue();

        _repoMock.Verify(r => r.Update(It.Is<Product>(r => r.Id == ProductId.Of(productId) && !r.IsEnabled.Equals(true))), Times.Once);
    }

    [Fact(DisplayName = "DELETE /api/products/{id} returns 404 when Product does not exist")]
    public async Task Delete_ShouldReturn404_WhenProductDoesNotExist()
    {
        // Arrange
        var productId = Guid.NewGuid();
        _repoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product)null); // Product not found

        // Act
        var response = await _client.DeleteAsync($"/api/products/{productId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

}
