using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Moq;
using wfc.referential.Domain.AffiliateAggregate;
using wfc.referential.Domain.CorridorAggregate;
using wfc.referential.Domain.PricingAggregate;
using wfc.referential.Domain.ServiceAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.PricingTests.GetByIdTests;

public class GetPricingByIdEndpointTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{
    private static Pricing Make(Guid id, string code = "PRICING-001", string? name = null, bool enabled = true)
    {
        var pricing = Pricing.Create(
            id: PricingId.Of(id),
            code: code,
            channel: "Online",
            minimumAmount: 10.00m,
            maximumAmount: 5000.00m,
            fixedAmount: 5.00m,
            rate: 0.05m,
            corridorId: CorridorId.Of(Guid.NewGuid()),
            serviceId: ServiceId.Of(Guid.NewGuid()),
            affiliateId: AffiliateId.Of(Guid.NewGuid())
        );

        if (!enabled)
            pricing.Disable();

        return pricing;
    }

    private record PricingDto(Guid Id, string Code, string Channel, bool IsEnabled);

    [Fact(DisplayName = "GET /api/pricings/{id} → 200 when Pricing exists")]
    public async Task Get_ShouldReturn200_WhenPricingExists()
    {
        var id = Guid.NewGuid();
        var entity = Make(id, "PRICING-123", "Standard Transfer Fee");

        _pricingRepoMock.Setup(r => r.GetByIdAsync(PricingId.Of(id), It.IsAny<CancellationToken>()))
             .ReturnsAsync(entity);

        var res = await _client.GetAsync($"/api/pricings/{id}");
        var body = await res.Content.ReadFromJsonAsync<PricingDto>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        body!.Id.Should().Be(id);
        body.Code.Should().Be("PRICING-123");
        body.Channel.Should().Be("Online");
        body.IsEnabled.Should().BeTrue();

        _pricingRepoMock.Verify(r => r.GetByIdAsync(PricingId.Of(id), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "GET /api/pricings/{id} → 404 when Pricing not found")]
    public async Task Get_ShouldReturn404_WhenPricingNotFound()
    {
        var id = Guid.NewGuid();

        _pricingRepoMock.Setup(r => r.GetByIdAsync(PricingId.Of(id), It.IsAny<CancellationToken>()))
             .ReturnsAsync((Pricing?)null);

        var res = await _client.GetAsync($"/api/pricings/{id}");
        var doc = await res.Content.ReadFromJsonAsync<JsonDocument>();

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var root = doc!.RootElement;
        root.GetProperty("title").GetString().Should().Be("Resource Not Found");
        root.GetProperty("status").GetInt32().Should().Be(404);

        _pricingRepoMock.Verify(r => r.GetByIdAsync(PricingId.Of(id), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "GET /api/pricings/{id} → 404 when id is malformed")]
    public async Task Get_ShouldReturn404_WhenIdMalformed()
    {
        const string bad = "not-a-guid";

        var res = await _client.GetAsync($"/api/pricings/{bad}");

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);

        _pricingRepoMock.Verify(r => r.GetByIdAsync(It.IsAny<PricingId>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "GET /api/pricings/{id} → 200 for disabled Pricing")]
    public async Task Get_ShouldReturn200_WhenPricingDisabled()
    {
        var id = Guid.NewGuid();
        var entity = Make(id, "PRICING-DIS", enabled: false);

        _pricingRepoMock.Setup(r => r.GetByIdAsync(PricingId.Of(id), It.IsAny<CancellationToken>()))
             .ReturnsAsync(entity);

        var res = await _client.GetAsync($"/api/pricings/{id}");
        var dto = await res.Content.ReadFromJsonAsync<PricingDto>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.IsEnabled.Should().BeFalse();
    }
}