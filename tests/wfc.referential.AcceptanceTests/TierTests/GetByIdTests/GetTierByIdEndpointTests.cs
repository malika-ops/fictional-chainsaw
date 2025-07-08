using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Moq;
using wfc.referential.Domain.TierAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.TierTests.GetByIdTests;

public class GetTierByIdEndpointTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{
    private static Tier Make(Guid id, string code = "TIER-001", string? name = null, bool enabled = true)
    {
        var tier = Tier.Create(TierId.Of(id), code, name ?? $"Tier-{code}");
        if (!enabled) tier.Disable();
        return tier;
    }

    private record TierDto(Guid Id, string Code, string Name, bool IsEnabled);

    [Fact(DisplayName = "GET /api/tiers/{id} → 404 when Tier not found")]
    public async Task Get_ShouldReturn404_WhenTierNotFound()
    {
        var id = Guid.NewGuid();

        _tierRepoMock.Setup(r => r.GetByIdAsync(TierId.Of(id), It.IsAny<CancellationToken>()))
             .ReturnsAsync((Tier?)null);

        var res = await _client.GetAsync($"/api/tiers/{id}");
        var doc = await res.Content.ReadFromJsonAsync<JsonDocument>();

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var root = doc!.RootElement;
        root.GetProperty("title").GetString().Should().Be("Resource Not Found");
        root.GetProperty("status").GetInt32().Should().Be(404);

        _tierRepoMock.Verify(r => r.GetByIdAsync(TierId.Of(id), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "GET /api/tiers/{id} → 404 when id is malformed")]
    public async Task Get_ShouldReturn404_WhenIdMalformed()
    {
        const string bad = "not-a-guid";

        var res = await _client.GetAsync($"/api/tiers/{bad}");

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);

        _tierRepoMock.Verify(r => r.GetByIdAsync(It.IsAny<TierId>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "GET /api/tiers/{id} → 200 for disabled Tier")]
    public async Task Get_ShouldReturn200_WhenTierDisabled()
    {
        var id = Guid.NewGuid();
        var entity = Make(id, "TIER-DIS", enabled: false);

        _tierRepoMock.Setup(r => r.GetByIdAsync(TierId.Of(id), It.IsAny<CancellationToken>()))
             .ReturnsAsync(entity);

        var res = await _client.GetAsync($"/api/tiers/{id}");
        var dto = await res.Content.ReadFromJsonAsync<TierDto>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.IsEnabled.Should().BeFalse();
    }
} 