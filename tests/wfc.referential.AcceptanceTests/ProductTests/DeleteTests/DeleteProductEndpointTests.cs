using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Moq;
using wfc.referential.Domain.ProductAggregate;
using wfc.referential.Domain.ServiceAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.ProductTests.DeleteTests;

public class DeleteProductEndpointTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{
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

        _productRepoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _serviceRepoMock.Setup(r => r.GetByConditionAsync(It.IsAny<Expression<Func<Service, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Service>()); // No service

        // Act
        var response = await _client.DeleteAsync($"/api/products/{productId}");
        var result = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeTrue();

        _productRepoMock.Verify(r => r.Update(It.Is<Product>(r => r.Id == ProductId.Of(productId) && !r.IsEnabled.Equals(true))), Times.Once);
    }

    [Fact(DisplayName = "DELETE /api/products/{id} returns 404 when Product does not exist")]
    public async Task Delete_ShouldReturn404_WhenProductDoesNotExist()
    {
        // Arrange
        var productId = Guid.NewGuid();
        _productRepoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product)null); // Product not found

        // Act
        var response = await _client.DeleteAsync($"/api/products/{productId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

}
