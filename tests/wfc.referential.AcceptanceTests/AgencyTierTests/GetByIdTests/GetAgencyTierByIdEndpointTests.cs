using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Moq;
using wfc.referential.Domain.AgencyAggregate;
using wfc.referential.Domain.AgencyTierAggregate;
using wfc.referential.Domain.TierAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.AgencyTierTests.GetByIdTests;

public class GetAgencyTierByIdEndpointTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{
    private static AgencyTier Make(Guid id, string code = "AGENCY-TIER-001", string? name = null, bool enabled = true)
    {
        var agencyTier = AgencyTier.Create(
            id: AgencyTierId.Of(id),
            agencyId: AgencyId.Of(Guid.NewGuid()), // Default agency ID
            tierId: TierId.Of(Guid.NewGuid()), // Default tier ID
            code: code,
            password: "default-password" // Default password
        );

        if (!enabled)
            agencyTier.Disable();

        return agencyTier;
    }

    private record AgencyTierDto(Guid Id, string Code, string Name, bool IsEnabled);

    [Fact(DisplayName = "GET /api/agencytiers/{id} → 404 when AgencyTier not found")]
    public async Task Get_ShouldReturn404_WhenAgencyTierNotFound()
    {
        var id = Guid.NewGuid();

        _agencyTierRepoMock.Setup(r => r.GetByIdAsync(AgencyTierId.Of(id), It.IsAny<CancellationToken>()))
             .ReturnsAsync((AgencyTier?)null);

        var res = await _client.GetAsync($"/api/agencytiers/{id}");
        var doc = await res.Content.ReadFromJsonAsync<JsonDocument>();

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var root = doc!.RootElement;
        root.GetProperty("title").GetString().Should().Be("Resource Not Found");
        root.GetProperty("status").GetInt32().Should().Be(404);

        _agencyTierRepoMock.Verify(r => r.GetByIdAsync(AgencyTierId.Of(id), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "GET /api/agencytiers/{id} → 404 when id is malformed")]
    public async Task Get_ShouldReturn404_WhenIdMalformed()
    {
        const string bad = "not-a-guid";

        var res = await _client.GetAsync($"/api/agencytiers/{bad}");

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);

        _agencyTierRepoMock.Verify(r => r.GetByIdAsync(It.IsAny<AgencyTierId>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "GET /api/agencytiers/{id} → 200 for disabled AgencyTier")]
    public async Task Get_ShouldReturn200_WhenAgencyTierDisabled()
    {
        var id = Guid.NewGuid();
        var entity = Make(id, "AGENCY-TIER-DIS", enabled: false);

        _agencyTierRepoMock.Setup(r => r.GetByIdAsync(AgencyTierId.Of(id), It.IsAny<CancellationToken>()))
             .ReturnsAsync(entity);

        var res = await _client.GetAsync($"/api/agencytiers/{id}");
        var dto = await res.Content.ReadFromJsonAsync<AgencyTierDto>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.IsEnabled.Should().BeFalse();
    }
}