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
using wfc.referential.Application.Products.Dtos;
using wfc.referential.Domain.ProductAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.ProductTests.PatchTests;

public class PatchProductEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IProductRepository> _repoMock = new();

    public PatchProductEndpointTests(WebApplicationFactory<Program> factory)
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

    [Fact(DisplayName = "PATCH /api/products/{id} updates the Product successfully")]
    public async Task PatchProduct_ShouldReturnUpdatedProductId_WhenProductExists()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var patchRequest = new PatchProductRequest
        {
            ProductId = productId,
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
        _repoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<CancellationToken>()))
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
            ProductId = productId,
            Code = "non-existing-code",
            Name = "Non-existing Product",
        };

        _repoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<CancellationToken>()))
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
            ProductId = productId,
            Code = "", // Assuming empty code is invalid
            Name = "Invalid Product",
        };

        _repoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Product.Create(ProductId.Of(productId), "code", "name", true));

        // Act
        var response = await _client.PatchAsync($"/api/products/{productId}", JsonContent.Create(patchRequest));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
