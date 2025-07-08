using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Moq;
using wfc.referential.Domain.ProductAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.ProductTests.GetByIdTests;

public class GetProductByIdEndpointTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{
    private static Product Make(Guid id, string code = "PRODUCT-001", string? name = null, bool enabled = true)
    {
        var product = Product.Create(
            id: ProductId.Of(id),
            code: code,
            name: name ?? $"Product-{code}",
            isEnabled: enabled
        );

        if (!enabled)
            product.SetInactive();

        return product;
    }

    private record ProductDto(Guid Id, string Code, string Name, bool IsEnabled);

    [Fact(DisplayName = "GET /api/products/{id} → 404 when Product not found")]
    public async Task Get_ShouldReturn404_WhenProductNotFound()
    {
        var id = Guid.NewGuid();

        _productRepoMock.Setup(r => r.GetByIdAsync(ProductId.Of(id), It.IsAny<CancellationToken>()))
             .ReturnsAsync((Product?)null);

        var res = await _client.GetAsync($"/api/products/{id}");
        var doc = await res.Content.ReadFromJsonAsync<JsonDocument>();

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var root = doc!.RootElement;
        root.GetProperty("title").GetString().Should().Be("Resource Not Found");
        root.GetProperty("status").GetInt32().Should().Be(404);

        _productRepoMock.Verify(r => r.GetByIdAsync(ProductId.Of(id), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "GET /api/products/{id} → 404 when id is malformed")]
    public async Task Get_ShouldReturn404_WhenIdMalformed()
    {
        const string bad = "not-a-guid";

        var res = await _client.GetAsync($"/api/products/{bad}");

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);

        _productRepoMock.Verify(r => r.GetByIdAsync(It.IsAny<ProductId>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "GET /api/products/{id} → 200 for disabled Product")]
    public async Task Get_ShouldReturn200_WhenProductDisabled()
    {
        var id = Guid.NewGuid();
        var entity = Make(id, "PRODUCT-DIS", enabled: false);

        _productRepoMock.Setup(r => r.GetByIdAsync(ProductId.Of(id), It.IsAny<CancellationToken>()))
             .ReturnsAsync(entity);

        var res = await _client.GetAsync($"/api/products/{id}");
        var dto = await res.Content.ReadFromJsonAsync<ProductDto>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.IsEnabled.Should().BeFalse();
    }
}