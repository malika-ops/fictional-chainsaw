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
using wfc.referential.Domain.CorridorAggregate;
using wfc.referential.Domain.PricingAggregate;
using wfc.referential.Domain.ServiceAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.PricingTests.DeleteTests;

public class DeletePricingEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IPricingRepository> _pricingRepoMock = new();

    public DeletePricingEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var customisedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IPricingRepository>();
                services.RemoveAll<ICacheService>();

                _pricingRepoMock
                    .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

                services.AddSingleton(_pricingRepoMock.Object);
                services.AddSingleton(cacheMock.Object);
            });
        });

        _client = customisedFactory.CreateClient();
    }


    private static Pricing MakePricing(Guid id, bool enabled = true)
    {
        var price = Pricing.Create(
            PricingId.Of(id),
            code: "CODE1",
            channel: "Branch",
            minimumAmount: 10,
            maximumAmount: 100,
            fixedAmount: 5,
            rate: null,
            corridorId: CorridorId.Of(Guid.NewGuid()),
            serviceId: ServiceId.Of(Guid.NewGuid()),
            affiliateId: null);

        if (!enabled) price.Disable();
        return price;
    }


    [Fact(DisplayName = "DELETE /api/pricings/{id} → 200 when Pricing exists")]
    public async Task Delete_ShouldReturn200_WhenPricingExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var pricing = MakePricing(id);

        _pricingRepoMock
            .Setup(r => r.GetByIdAsync(PricingId.Of(id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pricing);

        Pricing? captured = null;
        _pricingRepoMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() => captured = pricing)
            .Returns(Task.CompletedTask);

        // Act
        var resp = await _client.DeleteAsync($"/api/pricings/{id}");
        var body = await resp.Content.ReadFromJsonAsync<bool>();

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        body.Should().BeTrue();

        captured!.IsEnabled.Should().BeFalse();      // soft-deleted
        _pricingRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "DELETE /api/pricings/{id} → 404 when Pricing missing")]
    public async Task Delete_ShouldReturn404_WhenPricingNotFound()
    {
        var id = Guid.NewGuid();

        _pricingRepoMock
            .Setup(r => r.GetByIdAsync(PricingId.Of(id), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Pricing?)null);

        var resp = await _client.DeleteAsync($"/api/pricings/{id}");
        var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();

        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var root = doc!.RootElement;
        root.GetProperty("title").GetString().Should().Be("Resource Not Found");
        root.GetProperty("status").GetInt32().Should().Be(404);

        _pricingRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "DELETE /api/pricings/{id} → 400 when PricingId is empty GUID")]
    public async Task Delete_ShouldReturn400_WhenPricingIdEmpty()
    {
        var empty = Guid.Empty;

        var resp = await _client.DeleteAsync($"/api/pricings/{empty}");
        var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        _pricingRepoMock.Verify(r => r.GetByIdAsync(It.IsAny<PricingId>(), It.IsAny<CancellationToken>()), Times.Never);
        _pricingRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "DELETE /api/pricings/{id} → 400 when malformed GUID")]
    public async Task Delete_ShouldReturn400_WhenGuidMalformed()
    {
        const string badGuid = "not-a-guid";

        var resp = await _client.DeleteAsync($"/api/pricings/{badGuid}");

        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);

        _pricingRepoMock.Verify(r => r.GetByIdAsync(It.IsAny<PricingId>(), It.IsAny<CancellationToken>()), Times.Never);
        _pricingRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
