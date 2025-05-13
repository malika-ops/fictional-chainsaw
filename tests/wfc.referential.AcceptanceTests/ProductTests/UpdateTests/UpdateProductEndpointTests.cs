using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BuildingBlocks.Application.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.ProductAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.ProductTests.UpdateTests;

public class UpdateProductEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IProductRepository> _repoMock = new();


    public UpdateProductEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var customisedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IProductRepository>();
                services.RemoveAll<ICacheService>();

                // default noop for Update
                _repoMock
                    .Setup(r => r.UpdateProductAsync(It.IsAny<Product>(),
                                                          It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

                services.AddSingleton(_repoMock.Object);
                services.AddSingleton(cacheMock.Object);
            });
        });

        _client = customisedFactory.CreateClient();
    }

    // helper to create a Product quickly
    private static Product DummyProduct(Guid id, string code, string name) =>
        Product.Create(ProductId.Of(id), code, name, true);

    // 1) Happy‑path update
    [Fact(DisplayName = "PUT /api/products/{id} returns 200 when update succeeds")]
    public async Task Put_ShouldReturn200_WhenUpdateIsSuccessful()
    {
        // Arrange
        var id = Guid.NewGuid();
        var oldProduct = DummyProduct(id, "001", "Cash Express");

        _repoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(oldProduct);

        Product? updated = null;
        _repoMock.Setup(r => r.UpdateProductAsync(oldProduct,
                                                       It.IsAny<CancellationToken>()))
                 .Callback<Product, CancellationToken>((rg, _) => updated = rg)
                 .Returns(Task.CompletedTask);

        var payload = new
        {
            ProductId = id,
            Code = "codeAAB",
            Name= "nameAAB"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/products/{id}", payload);
        var returned = await response.Content.ReadFromJsonAsync<Guid>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returned.Should().Be(id);

        updated!.Code.Should().Be("codeAAB");
        updated.Name.Should().Be("nameAAB");

        _repoMock.Verify(r => r.UpdateProductAsync(It.IsAny<Product>(),
                                                        It.IsAny<CancellationToken>()),
                         Times.Once);
    }

    // 2) Validation error – Name missing
    [Fact(DisplayName = "PUT /api/products/{id} returns 400 when Name is missing")]
    public async Task Put_ShouldReturn400_WhenNameMissing()
    {
        // Arrange
        var id = Guid.NewGuid();
        var payload = new
        {
            Code = "codeAAB"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/Products/{id}", payload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors")
            .GetProperty("name")[0].GetString()
            .Should().Be("Name is required");

        _repoMock.Verify(r => r.UpdateProductAsync(It.IsAny<Product>(),
                                                        It.IsAny<CancellationToken>()),
                         Times.Never);
    }

    //// 3) Duplicate code
    [Fact(DisplayName = "PUT /api/Products/{id} returns 400 when new code already exists")]
    public async Task Put_ShouldReturn400_WhenCodeAlreadyExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existing = DummyProduct(Guid.NewGuid(), "001", "Cash Express");
        var target = DummyProduct(id, "002", "Floussy");

        _repoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(target);

        _repoMock.Setup(r => r.GetByCodeAsync("002", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(existing); // duplicate code

        var payload = new
        {
            ProductId = id,
            Code = "002",          // duplicate
            Name = "Floussy"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/Products/{id}", payload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors").GetString()
           .Should().Be($"{nameof(Product)} with code : {payload.Code} already exist");

        _repoMock.Verify(r => r.UpdateProductAsync(It.IsAny<Product>(),
                                                        It.IsAny<CancellationToken>()),
                         Times.Never);
    }
}
