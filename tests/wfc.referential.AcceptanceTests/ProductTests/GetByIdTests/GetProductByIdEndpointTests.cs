using BuildingBlocks.Application.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.ProductAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.ProductTests.GetByIdTests;

public class GetProductByIdEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IProductRepository> _repo = new();

    public GetProductByIdEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var custom = factory.WithWebHostBuilder(b =>
        {
            b.UseEnvironment("Testing");

            b.ConfigureServices(s =>
            {
                s.RemoveAll<IProductRepository>();
                s.RemoveAll<ICacheService>();

                s.AddSingleton(_repo.Object);
                s.AddSingleton(cacheMock.Object);
            });
        });

        _client = custom.CreateClient();
    }

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

        _repo.Setup(r => r.GetByIdAsync(ProductId.Of(id), It.IsAny<CancellationToken>()))
             .ReturnsAsync((Product?)null);

        var res = await _client.GetAsync($"/api/products/{id}");
        var doc = await res.Content.ReadFromJsonAsync<JsonDocument>();

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var root = doc!.RootElement;
        root.GetProperty("title").GetString().Should().Be("Resource Not Found");
        root.GetProperty("status").GetInt32().Should().Be(404);

        _repo.Verify(r => r.GetByIdAsync(ProductId.Of(id), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "GET /api/products/{id} → 404 when id is malformed")]
    public async Task Get_ShouldReturn404_WhenIdMalformed()
    {
        const string bad = "not-a-guid";

        var res = await _client.GetAsync($"/api/products/{bad}");

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);

        _repo.Verify(r => r.GetByIdAsync(It.IsAny<ProductId>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "GET /api/products/{id} → 200 for disabled Product")]
    public async Task Get_ShouldReturn200_WhenProductDisabled()
    {
        var id = Guid.NewGuid();
        var entity = Make(id, "PRODUCT-DIS", enabled: false);

        _repo.Setup(r => r.GetByIdAsync(ProductId.Of(id), It.IsAny<CancellationToken>()))
             .ReturnsAsync(entity);

        var res = await _client.GetAsync($"/api/products/{id}");
        var dto = await res.Content.ReadFromJsonAsync<ProductDto>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.IsEnabled.Should().BeFalse();
    }
}