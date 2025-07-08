using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Moq;
using wfc.referential.Application.Products.Dtos;
using wfc.referential.Domain.ProductAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.ProductTests.PatchTests;

public class PatchProductEndpointTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{
    [Fact(DisplayName = "PATCH /api/products/{id} updates the Product successfully")]
    public async Task PatchProduct_ShouldReturnUpdatedProductId_WhenProductExists()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var patchRequest = new PatchProductRequest
        {
            Code = "new-code",
            Name = "Updated Name",
            IsEnabled = true
        };

        var product = Product.Create(
            ProductId.Of(productId),
            "old-code",
            "Old Name",
            true
        );
        _productRepoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Expression<Func<Product, bool>> predicate, CancellationToken _) =>
            {
                var func = predicate.Compile();

                if (func(product))
                    return product;

                return null;
            });


        // Act
        var response = await _client.PatchAsync($"/api/products/{productId}", JsonContent.Create(patchRequest));
        var updatedProductId = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        updatedProductId.Should().Be(true);
        product.Name.Should().BeEquivalentTo(patchRequest.Name);
    }


    [Fact(DisplayName = "PATCH /api/products/{id} returns 404 when Product does not exist")]
        public async Task PatchProduct_ShouldReturnNotFound_WhenProductDoesNotExist()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var patchRequest = new PatchProductRequest
        {
            Code = "non-existing-code",
            Name = "Non-existing Product",
        };

        _productRepoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product)null); 

        // Act
        var response = await _client.PatchAsync($"/api/Products/{productId}", JsonContent.Create(patchRequest));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "PATCH /api/products/{id} returns 400 when validation fails")]
    public async Task PatchProduct_ShouldReturnBadRequest_WhenValidationFails()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var patchRequest = new PatchProductRequest
        {
            Code = "", // Assuming empty code is invalid
            Name = "Invalid Product",
        };

        _productRepoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Product.Create(ProductId.Of(productId), "code", "name", true));

        // Act
        var response = await _client.PatchAsync($"/api/products/{productId}", JsonContent.Create(patchRequest));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
