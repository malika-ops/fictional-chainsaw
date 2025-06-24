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
using System.Text;
using System.Text.Json;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.AffiliateAggregate;
using wfc.referential.Domain.CorridorAggregate;
using wfc.referential.Domain.PricingAggregate;
using wfc.referential.Domain.ServiceAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.PricingTests.PatchTests;

public class PatchPricingEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IPricingRepository> _pricingRepo = new();
    private readonly Mock<IServiceRepository> _serviceRepo = new();
    private readonly Mock<ICorridorRepository> _corridorRepo = new();
    private readonly Mock<IAffiliateRepository> _affiliateRepo = new();

    public PatchPricingEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var custom = factory.WithWebHostBuilder(b =>
        {
            b.UseEnvironment("Testing");
            b.ConfigureServices(s =>
            {
                s.RemoveAll<IPricingRepository>();
                s.RemoveAll<IServiceRepository>();
                s.RemoveAll<ICorridorRepository>();
                s.RemoveAll<IAffiliateRepository>();
                s.RemoveAll<ICacheService>();

                _pricingRepo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                            .Returns(Task.CompletedTask);

                s.AddSingleton(_pricingRepo.Object);
                s.AddSingleton(_serviceRepo.Object);
                s.AddSingleton(_corridorRepo.Object);
                s.AddSingleton(_affiliateRepo.Object);
                s.AddSingleton(cacheMock.Object);
            });
        });

        _client = custom.CreateClient();
    }


    private static Pricing MakePricing(Guid id,
                                       string code = "CODE1",
                                       string chan = "Branch",
                                       decimal min = 10,
                                       decimal max = 100,
                                       decimal? fix = 5,
                                       decimal? rate = null,
                                       Guid? corId = null,
                                       Guid? svcId = null,
                                       Guid? affId = null,
                                       bool enabled = true)
    {
        var price = Pricing.Create(
            PricingId.Of(id),
            code,
            chan,
            min,
            max,
            fix,
            rate,
            CorridorId.Of(corId ?? Guid.NewGuid()),
            ServiceId.Of(svcId ?? Guid.NewGuid()),
            affId is { } a ? AffiliateId.Of(a) : null);

        if (!enabled) price.Disable();
        return price;
    }

    private static async Task<HttpResponseMessage> PatchJsonAsync(
        HttpClient client, string url, object body)
    {
        var json = JsonSerializer.Serialize(body);
        var req = new HttpRequestMessage(HttpMethod.Patch, url)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        return await client.SendAsync(req);
    }

    private static async Task<bool> ReadBoolAsync(HttpResponseMessage resp)
    {
        var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();
        var root = doc!.RootElement;

        if (root.ValueKind is JsonValueKind.True or JsonValueKind.False)
            return root.GetBoolean();

        if (root.TryGetProperty("value", out var v) &&
            (v.ValueKind is JsonValueKind.True or JsonValueKind.False))
            return v.GetBoolean();

        return root.GetBoolean();
    }


    [Fact(DisplayName = "PATCH /api/pricings/{id} → 200 when patching only Code")]
    public async Task Patch_ShouldReturn200_WhenPatchingOnlyCode()
    {
        var id = Guid.NewGuid();
        var price = MakePricing(id, "OLD");

        _pricingRepo.Setup(r => r.GetByIdAsync(PricingId.Of(id), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(price);

        _pricingRepo.Setup(r => r.GetOneByConditionAsync(
                              It.IsAny<Expression<Func<Pricing, bool>>>(),
                              It.IsAny<CancellationToken>()))
                    .ReturnsAsync((Pricing?)null);

        var payload = new { PricingId = id, Code = "NEW" };

        var resp = await PatchJsonAsync(_client, $"/api/pricings/{id}", payload);
        var ok = await ReadBoolAsync(resp);

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        ok.Should().BeTrue();
        price.Code.Should().Be("NEW");

        _pricingRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PATCH /api/pricings/{id} → 200 when disabling pricing")]
    public async Task Patch_ShouldReturn200_WhenPatchingIsEnabled()
    {
        var id = Guid.NewGuid();
        var price = MakePricing(id);

        _pricingRepo.Setup(r => r.GetByIdAsync(PricingId.Of(id), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(price);

        var payload = new { PricingId = id, IsEnabled = false };

        var resp = await PatchJsonAsync(_client, $"/api/pricings/{id}", payload);
        var ok = await ReadBoolAsync(resp);

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        ok.Should().BeTrue();
        price.IsEnabled.Should().BeFalse();
    }

    [Fact(DisplayName = "PATCH /api/pricings/{id} → 404 when Pricing missing")]
    public async Task Patch_ShouldReturn404_WhenPricingNotFound()
    {
        var id = Guid.NewGuid();

        _pricingRepo.Setup(r => r.GetByIdAsync(PricingId.Of(id), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((Pricing?)null);

        var resp = await PatchJsonAsync(_client, $"/api/pricings/{id}", new { PricingId = id });
        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);

        _pricingRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

   
    [Fact(DisplayName = "PATCH /api/pricings/{id} → 409 when duplicate Code exists")]
    public async Task Patch_ShouldReturn409_WhenDuplicateCode()
    {
        var id = Guid.NewGuid();
        var otherId = Guid.NewGuid();
        var dupCode = "DUP";

        var price = MakePricing(id, "OLD");
        var dupe = MakePricing(otherId, dupCode);

        _pricingRepo.Setup(r => r.GetByIdAsync(PricingId.Of(id), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(price);

        _pricingRepo.Setup(r => r.GetOneByConditionAsync(
                              It.IsAny<Expression<Func<Pricing, bool>>>(),
                              It.IsAny<CancellationToken>()))
                    .ReturnsAsync(dupe); // conflict

        var resp = await PatchJsonAsync(_client, $"/api/pricings/{id}", new { PricingId = id, Code = dupCode });
        resp.StatusCode.Should().Be(HttpStatusCode.Conflict);

        _pricingRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PATCH /api/pricings/{id} → 400 when Code > 50 chars")]
    public async Task Patch_ShouldReturn400_WhenCodeTooLong()
    {
        var id = Guid.NewGuid();
        var longCode = new string('C', 51);

        var resp = await PatchJsonAsync(_client, $"/api/pricings/{id}",
                                        new { PricingId = id, Code = longCode });
        var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        doc!.RootElement.GetProperty("errors")
           .GetProperty("Code")[0].GetString()
           .Should().Be("Code max length = 50.");

        _pricingRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PATCH /api/pricings/{id} → 400 when Max ≤ Min")]
    public async Task Patch_ShouldReturn400_WhenAmountsInvalid()
    {
        var id = Guid.NewGuid();

        var payload = new
        {
            PricingId = id,
            MinimumAmount = 100m,
            MaximumAmount = 80m
        };

        var resp = await PatchJsonAsync(_client, $"/api/pricings/{id}", payload);
        var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        _pricingRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PATCH /api/pricings/{id} allows same Code for same row")]
    public async Task Patch_ShouldAllow_WhenSameCodeSameEntity()
    {
        var id = Guid.NewGuid();
        var price = MakePricing(id, "SAME");

        _pricingRepo.Setup(r => r.GetByIdAsync(PricingId.Of(id), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(price);

        _pricingRepo.Setup(r => r.GetOneByConditionAsync(
                              It.IsAny<Expression<Func<Pricing, bool>>>(),
                              It.IsAny<CancellationToken>()))
                    .ReturnsAsync(price); // same entity

        var resp = await PatchJsonAsync(_client, $"/api/pricings/{id}", new { PricingId = id, Code = "SAME" });
        var ok = await ReadBoolAsync(resp);

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        ok.Should().BeTrue();

        _pricingRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}