using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Moq;
using wfc.referential.Domain.AgencyAggregate;
using wfc.referential.Domain.CityAggregate;
using wfc.referential.Domain.CorridorAggregate;
using wfc.referential.Domain.Countries;
using Xunit;

namespace wfc.referential.AcceptanceTests.CorridorTests.GetByIdTests;

public class GetCorridorByIdEndpointTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{
    private static Corridor Make(Guid id, string code = "CORRIDOR-001", string? name = null, bool enabled = true)
    {
        var corridor = Corridor.Create(
            id: CorridorId.Of(id),
            sourceCountry: CountryId.Of(Guid.NewGuid()),
            destCountry: CountryId.Of(Guid.NewGuid()),
            sourceCity: CityId.Of(Guid.NewGuid()),
            destCity: CityId.Of(Guid.NewGuid()),
            sourceBranch: AgencyId.Of(Guid.NewGuid()),
            destBranch: AgencyId.Of(Guid.NewGuid())
        );

        if (!enabled)
            corridor.SetInactive();

        return corridor;
    }

    private record CorridorDto(Guid Id, Guid? SourceCountryId, Guid? DestinationCountryId, bool IsEnabled);

    [Fact(DisplayName = "GET /api/corridors/{id} → 404 when Corridor not found")]
    public async Task Get_ShouldReturn404_WhenCorridorNotFound()
    {
        var id = Guid.NewGuid();

        _corridorRepoMock.Setup(r => r.GetByIdAsync(CorridorId.Of(id), It.IsAny<CancellationToken>()))
             .ReturnsAsync((Corridor?)null);

        var res = await _client.GetAsync($"/api/corridors/{id}");
        var doc = await res.Content.ReadFromJsonAsync<JsonDocument>();

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var root = doc!.RootElement;
        root.GetProperty("title").GetString().Should().Be("Resource Not Found");
        root.GetProperty("status").GetInt32().Should().Be(404);

        _corridorRepoMock.Verify(r => r.GetByIdAsync(CorridorId.Of(id), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "GET /api/corridors/{id} → 404 when id is malformed")]
    public async Task Get_ShouldReturn404_WhenIdMalformed()
    {
        const string bad = "not-a-guid";

        var res = await _client.GetAsync($"/api/corridors/{bad}");

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);

        _corridorRepoMock.Verify(r => r.GetByIdAsync(It.IsAny<CorridorId>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "GET /api/corridors/{id} → 200 for disabled Corridor")]
    public async Task Get_ShouldReturn200_WhenCorridorDisabled()
    {
        var id = Guid.NewGuid();
        var entity = Make(id, "CORRIDOR-DIS", enabled: false);

        _corridorRepoMock.Setup(r => r.GetByIdAsync(CorridorId.Of(id), It.IsAny<CancellationToken>()))
             .ReturnsAsync(entity);

        var res = await _client.GetAsync($"/api/corridors/{id}");
        var dto = await res.Content.ReadFromJsonAsync<CorridorDto>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.IsEnabled.Should().BeFalse();
    }
}