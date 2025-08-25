using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Moq;
using wfc.referential.Domain.AffiliateAggregate;
using wfc.referential.Domain.Countries;
using Xunit;

namespace wfc.referential.AcceptanceTests.AffiliatesTests.GetByIdTests;

public class GetAffiliateByIdEndpointTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{
    private static Affiliate Make(Guid id, string code = "AFF001", string? name = null, bool enabled = true)
    {
        var affiliate = Affiliate.Create(
            AffiliateId.Of(id),
            code,
            name ?? $"Affiliate-{code}",
            "WFC",
            DateTime.Now.AddDays(-30),
            "Last day of month",
            "/logos/affiliate.png",
            10000.00m,
            "ACC-DOC-001",
            "411000001",
            "Stamp duty applicable",
            AffiliateTypeEnum.Paycash,
            CountryId.Of(Guid.NewGuid()));

        if (!enabled) affiliate.Disable();
        return affiliate;
    }

    private record AffiliateDto(Guid AffiliateId, string Code, string Name, string Abbreviation, bool IsEnabled);

    [Fact(DisplayName = "GET /api/affiliates/{id} → 200 when Affiliate exists")]
    public async Task Get_ShouldReturn200_WhenAffiliateExists()
    {
        var id = Guid.NewGuid();
        var entity = Make(id, "AFF001", "Wafacash");

        _affiliateRepoMock.Setup(r => r.GetByIdAsync(AffiliateId.Of(id), It.IsAny<CancellationToken>()))
             .ReturnsAsync(entity);

        var res = await _client.GetAsync($"/api/affiliates/{id}");
        var body = await res.Content.ReadFromJsonAsync<AffiliateDto>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        body!.AffiliateId.Should().Be(id);
        body.Code.Should().Be("AFF001");
        body.Name.Should().Be("Wafacash");
        body.IsEnabled.Should().BeTrue();

        _affiliateRepoMock.Verify(r => r.GetByIdAsync(AffiliateId.Of(id), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "GET /api/affiliates/{id} → 404 when Affiliate not found")]
    public async Task Get_ShouldReturn404_WhenAffiliateNotFound()
    {
        var id = Guid.NewGuid();

        _affiliateRepoMock.Setup(r => r.GetByIdAsync(AffiliateId.Of(id), It.IsAny<CancellationToken>()))
             .ReturnsAsync((Affiliate?)null);

        var res = await _client.GetAsync($"/api/affiliates/{id}");
        var doc = await res.Content.ReadFromJsonAsync<JsonDocument>();

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var root = doc!.RootElement;
        root.GetProperty("title").GetString().Should().Be("Resource Not Found");
        root.GetProperty("status").GetInt32().Should().Be(404);

        _affiliateRepoMock.Verify(r => r.GetByIdAsync(AffiliateId.Of(id), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "GET /api/affiliates/{id} → 404 when id is malformed")]
    public async Task Get_ShouldReturn404_WhenIdMalformed()
    {
        const string bad = "not-a-guid";

        var res = await _client.GetAsync($"/api/affiliates/{bad}");

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);

        _affiliateRepoMock.Verify(r => r.GetByIdAsync(It.IsAny<AffiliateId>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "GET /api/affiliates/{id} → 200 for disabled Affiliate")]
    public async Task Get_ShouldReturn200_WhenAffiliateDisabled()
    {
        var id = Guid.NewGuid();
        var entity = Make(id, "AFF-DIS", enabled: false);

        _affiliateRepoMock.Setup(r => r.GetByIdAsync(AffiliateId.Of(id), It.IsAny<CancellationToken>()))
             .ReturnsAsync(entity);

        var res = await _client.GetAsync($"/api/affiliates/{id}");
        var dto = await res.Content.ReadFromJsonAsync<AffiliateDto>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.IsEnabled.Should().BeFalse();
    }
}