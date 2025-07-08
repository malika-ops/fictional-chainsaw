using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AutoFixture;
using FluentAssertions;
using Moq;
using wfc.referential.Domain.AffiliateAggregate;
using wfc.referential.Domain.CorridorAggregate;
using wfc.referential.Domain.PricingAggregate;
using wfc.referential.Domain.ServiceAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.PricingTests.UpdateTests;

public class UpdatePricingEndpointTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{
    private static Pricing CreatePricing(
        Guid id,
        string code,
        string channel,
        decimal min,
        decimal max,
        decimal? fixedAmt,
        decimal? rate,
        Guid corridorId,
        Guid serviceId,
        Guid? affiliateId = null,
        bool enabled = true)
    {
        var price = Pricing.Create(
            PricingId.Of(id),
            code,
            channel,
            min,
            max,
            fixedAmt,
            rate,
            CorridorId.Of(corridorId),
            ServiceId.Of(serviceId),
            affiliateId is { } a ? AffiliateId.Of(a) : null);

        if (!enabled) price.Disable();
        return price;
    }

    [Fact(DisplayName = "PUT /api/pricings/{id} → 200 when update succeeds")]
    public async Task Put_ShouldReturn200_WhenUpdateSuccessful()
    {
        // Arrange
        var id = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var corridorId = Guid.NewGuid();
        var affiliateId = Guid.NewGuid();

        var pricing = CreatePricing(
            id, "OLD", "Branch", 10, 100, 5, null, corridorId, serviceId, affiliateId);

        _pricingRepoMock.Setup(r => r.GetByIdAsync(PricingId.Of(id), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(pricing);

        _serviceRepoMock.Setup(r => r.GetByIdAsync(ServiceId.Of(serviceId), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(_fixture.Create<Service>());

        _corridorRepoMock.Setup(r => r.GetByIdAsync(CorridorId.Of(corridorId), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(_fixture.Create<Corridor>());

        _affiliateRepoMock.Setup(r => r.GetByIdAsync(AffiliateId.Of(affiliateId), It.IsAny<CancellationToken>()))
                          .ReturnsAsync(_fixture.Create<Affiliate>());

        _pricingRepoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<
                               System.Linq.Expressions.Expression<Func<Pricing, bool>>>(),
                               It.IsAny<CancellationToken>()))
                        .ReturnsAsync(pricing); 

        var payload = new
        {
            PricingId = id,
            Code = "NEW",
            Channel = "Online",
            MinimumAmount = 20m,
            MaximumAmount = 200m,
            FixedAmount = 7m,
            Rate = (decimal?)null,
            CorridorId = corridorId,
            ServiceId = serviceId,
            AffiliateId = affiliateId,
            IsEnabled = false
        };

        // Act
        var res = await _client.PutAsJsonAsync($"/api/pricings/{id}", payload);
        var ok = await res.Content.ReadFromJsonAsync<bool>();

        // Assert
        res.StatusCode.Should().Be(HttpStatusCode.OK);
        ok.Should().BeTrue();

        pricing.Code.Should().Be("NEW");
        pricing.Channel.Should().Be("Online");
        pricing.MinimumAmount.Should().Be(20m);
        pricing.MaximumAmount.Should().Be(200m);
        pricing.FixedAmount.Should().Be(7m);
        pricing.Rate.Should().BeNull();
        pricing.IsEnabled.Should().BeFalse();

        _pricingRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PUT /api/pricings/{id} → 400 when Code > 50 chars")]
    public async Task Put_ShouldReturn400_WhenCodeTooLong()
    {
        var id = Guid.NewGuid();
        var longCode = new string('C', 51);

        var payload = new
        {
            PricingId = id,
            Code = longCode,
            Channel = "Branch",
            MinimumAmount = 10m,
            MaximumAmount = 20m,
            FixedAmount = 1m,
            CorridorId = Guid.NewGuid(),
            ServiceId = Guid.NewGuid()
        };

        var res = await _client.PutAsJsonAsync($"/api/pricings/{id}", payload);
        var doc = await res.Content.ReadFromJsonAsync<JsonDocument>();

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        doc!.RootElement.GetProperty("errors")
            .GetProperty("Code")[0].GetString()
            .Should().Be("Code max length = 50.");

        _pricingRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PUT /api/pricings/{id} → 400 when MaximumAmount ≤ MinimumAmount")]
    public async Task Put_ShouldReturn400_WhenAmountsInvalid()
    {
        var id = Guid.NewGuid();

        var payload = new
        {
            PricingId = id,
            Code = "OK",
            Channel = "Branch",
            MinimumAmount = 100m,
            MaximumAmount = 90m,     // invalid
            FixedAmount = 1m,
            CorridorId = Guid.NewGuid(),
            ServiceId = Guid.NewGuid()
        };

        var res = await _client.PutAsJsonAsync($"/api/pricings/{id}", payload);
        var doc = await res.Content.ReadFromJsonAsync<JsonDocument>();

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        _pricingRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PUT /api/pricings/{id} → 400 when neither FixedAmount nor Rate supplied")]
    public async Task Put_ShouldReturn400_WhenBothFeesMissing()
    {
        var id = Guid.NewGuid();

        var payload = new
        {
            PricingId = id,
            Code = "OK",
            Channel = "Branch",
            MinimumAmount = 10m,
            MaximumAmount = 20m,
            FixedAmount = (decimal?)null,
            Rate = (decimal?)null,
            CorridorId = Guid.NewGuid(),
            ServiceId = Guid.NewGuid()
        };

        var res = await _client.PutAsJsonAsync($"/api/pricings/{id}", payload);
        var doc = await res.Content.ReadFromJsonAsync<JsonDocument>();

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        _pricingRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PUT /api/pricings/{id} → 404 when Pricing missing")]
    public async Task Put_ShouldReturn404_WhenPricingNotFound()
    {
        var id = Guid.NewGuid();
        _pricingRepoMock.Setup(r => r.GetByIdAsync(PricingId.Of(id), It.IsAny<CancellationToken>()))
                        .ReturnsAsync((Pricing?)null);

        var payload = new
        {
            PricingId = id,
            Code = "OK",
            Channel = "Branch",
            MinimumAmount = 10m,
            MaximumAmount = 20m,
            FixedAmount = 1m,
            CorridorId = Guid.NewGuid(),
            ServiceId = Guid.NewGuid()
        };

        var res = await _client.PutAsJsonAsync($"/api/pricings/{id}", payload);
        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Theory(DisplayName = "PUT /api/pricings/{id} → 404 when FK missing")]
    [InlineData("Service")]
    [InlineData("Corridor")]
    [InlineData("Affiliate")]
    public async Task Put_ShouldReturn404_WhenForeignKeyMissing(string missing)
    {
        var id = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var corridorId = Guid.NewGuid();
        var affiliateId = Guid.NewGuid();

        var pricing = CreatePricing(id, "OK", "Branch", 10, 20, 1, null, corridorId, serviceId, affiliateId);

        _pricingRepoMock.Setup(r => r.GetByIdAsync(PricingId.Of(id), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(pricing);

        _serviceRepoMock.Setup(r => r.GetByIdAsync(ServiceId.Of(serviceId), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(missing == "Service" ? null
                                      : _fixture.Create<Service>());

        _corridorRepoMock.Setup(r => r.GetByIdAsync(CorridorId.Of(corridorId), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(missing == "Corridor" ? null
                                      : _fixture.Create<Corridor>());

        _affiliateRepoMock.Setup(r => r.GetByIdAsync(AffiliateId.Of(affiliateId), It.IsAny<CancellationToken>()))
                          .ReturnsAsync(missing == "Affiliate" ? null
                                      : _fixture.Create<Affiliate>());

        _pricingRepoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<
                               System.Linq.Expressions.Expression<Func<Pricing, bool>>>(),
                               It.IsAny<CancellationToken>()))
                        .ReturnsAsync(pricing);

        var payload = new
        {
            PricingId = id,
            Code = "OK",
            Channel = "Branch",
            MinimumAmount = 10m,
            MaximumAmount = 20m,
            FixedAmount = 1m,
            CorridorId = corridorId,
            ServiceId = serviceId,
            AffiliateId = affiliateId
        };

        var res = await _client.PutAsJsonAsync($"/api/pricings/{id}", payload);
        res.StatusCode.Should().Be(HttpStatusCode.NotFound);

        _pricingRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PUT /api/pricings/{id} → 409 when duplicate Code exists")]
    public async Task Put_ShouldReturn409_WhenDuplicateCode()
    {
        var id = Guid.NewGuid();
        var otherId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var corridorId = Guid.NewGuid();

        var pricing = CreatePricing(id, "OLD", "Branch", 10, 20, 1, null, corridorId, serviceId);
        var conflicting = CreatePricing(otherId, "NEW", "Branch", 10, 20, 1, null, corridorId, serviceId);

        _pricingRepoMock.Setup(r => r.GetByIdAsync(PricingId.Of(id), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(pricing);

        _serviceRepoMock.Setup(r => r.GetByIdAsync(ServiceId.Of(serviceId), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(_fixture.Create<Service>());

        _corridorRepoMock.Setup(r => r.GetByIdAsync(CorridorId.Of(corridorId), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(_fixture.Create<Corridor>());

        _pricingRepoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<
                               System.Linq.Expressions.Expression<Func<Pricing, bool>>>(),
                               It.IsAny<CancellationToken>()))
                        .ReturnsAsync(conflicting); 

        var payload = new
        {
            PricingId = id,
            Code = "NEW", // duplicate
            Channel = "Branch",
            MinimumAmount = 10m,
            MaximumAmount = 20m,
            FixedAmount = 1m,
            CorridorId = corridorId,
            ServiceId = serviceId
        };

        var res = await _client.PutAsJsonAsync($"/api/pricings/{id}", payload);
        res.StatusCode.Should().Be(HttpStatusCode.Conflict);

        _pricingRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PUT /api/pricings/{id} → 200 when disabling pricing")]
    public async Task Put_ShouldReturn200_WhenDisabling()
    {
        var id = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var corridorId = Guid.NewGuid();

        var pricing = CreatePricing(id, "CODE", "Branch", 10, 20, 1, null, corridorId, serviceId, enabled: true);

        _pricingRepoMock.Setup(r => r.GetByIdAsync(PricingId.Of(id), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(pricing);

        _serviceRepoMock.Setup(r => r.GetByIdAsync(ServiceId.Of(serviceId), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(_fixture.Create<Service>());

        _corridorRepoMock.Setup(r => r.GetByIdAsync(CorridorId.Of(corridorId), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(_fixture.Create<Corridor>());

        _pricingRepoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<
                               System.Linq.Expressions.Expression<Func<Pricing, bool>>>(),
                               It.IsAny<CancellationToken>()))
                        .ReturnsAsync(pricing);

        var payload = new
        {
            PricingId = id,
            Code = "CODE",
            Channel = "Branch",
            MinimumAmount = 10m,
            MaximumAmount = 20m,
            FixedAmount = 1m,
            CorridorId = corridorId,
            ServiceId = serviceId,
            IsEnabled = false
        };

        var res = await _client.PutAsJsonAsync($"/api/pricings/{id}", payload);
        var ok = await res.Content.ReadFromJsonAsync<bool>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        ok.Should().BeTrue();
        pricing.IsEnabled.Should().BeFalse();
    }

    [Fact(DisplayName = "PUT /api/pricings/{id} → 200 when keeping same Code")]
    public async Task Put_ShouldReturn200_WhenKeepingSameCode()
    {
        var id = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var corridorId = Guid.NewGuid();

        var pricing = CreatePricing(id, "SAME", "Branch", 10, 20, 1, null, corridorId, serviceId);

        _pricingRepoMock.Setup(r => r.GetByIdAsync(PricingId.Of(id), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(pricing);

        _serviceRepoMock.Setup(r => r.GetByIdAsync(ServiceId.Of(serviceId), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(_fixture.Create<Service>());

        _corridorRepoMock.Setup(r => r.GetByIdAsync(CorridorId.Of(corridorId), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(_fixture.Create<Corridor>());

        _pricingRepoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<
                               System.Linq.Expressions.Expression<Func<Pricing, bool>>>(),
                               It.IsAny<CancellationToken>()))
                        .ReturnsAsync(pricing); 

        var payload = new
        {
            PricingId = id,
            Code = "SAME", 
            Channel = "Online",
            MinimumAmount = 15m,
            MaximumAmount = 30m,
            FixedAmount = 2m,
            CorridorId = corridorId,
            ServiceId = serviceId
        };

        var res = await _client.PutAsJsonAsync($"/api/pricings/{id}", payload);
        var ok = await res.Content.ReadFromJsonAsync<bool>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        ok.Should().BeTrue();

        pricing.Channel.Should().Be("Online");
        pricing.MinimumAmount.Should().Be(15m);

        _pricingRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
