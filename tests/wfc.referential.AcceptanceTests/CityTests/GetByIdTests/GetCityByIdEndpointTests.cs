using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Moq;
using wfc.referential.Domain.CityAggregate;
using wfc.referential.Domain.RegionAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.CityTests.GetByIdTests;

public class GetCityByIdEndpointTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{
    private static City Make(Guid id, string code = "CITY-001", string? name = null, bool enabled = true)
    {
        var city = City.Create(
            id: CityId.Of(id),
            cityCode: code,
            cityName: name ?? $"City-{code}",
            timeZone: "UTC", // Default timezone
            regionId: RegionId.Of(Guid.NewGuid()), // Default region ID
            abbreviation: code.Substring(0, Math.Min(3, code.Length)) // Default abbreviation from code
        );

        if (!enabled)
            city.SetInactive();

        return city;
    }

    private record CityDto(Guid Id, string Code, string Name, bool IsEnabled);

    [Fact(DisplayName = "GET /api/cities/{id} → 404 when City not found")]
    public async Task Get_ShouldReturn404_WhenCityNotFound()
    {
        var id = Guid.NewGuid();

        _cityRepoMock.Setup(r => r.GetByIdAsync(CityId.Of(id), It.IsAny<CancellationToken>()))
             .ReturnsAsync((City?)null);

        var res = await _client.GetAsync($"/api/cities/{id}");
        var doc = await res.Content.ReadFromJsonAsync<JsonDocument>();

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var root = doc!.RootElement;
        root.GetProperty("title").GetString().Should().Be("Resource Not Found");
        root.GetProperty("status").GetInt32().Should().Be(404);

        _cityRepoMock.Verify(r => r.GetByIdAsync(CityId.Of(id), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "GET /api/cities/{id} → 404 when id is malformed")]
    public async Task Get_ShouldReturn404_WhenIdMalformed()
    {
        const string bad = "not-a-guid";

        var res = await _client.GetAsync($"/api/cities/{bad}");

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);

        _cityRepoMock.Verify(r => r.GetByIdAsync(It.IsAny<CityId>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "GET /api/cities/{id} → 200 for disabled City")]
    public async Task Get_ShouldReturn200_WhenCityDisabled()
    {
        var id = Guid.NewGuid();
        var entity = Make(id, "CITY-DIS", enabled: false);

        _cityRepoMock.Setup(r => r.GetByIdAsync(CityId.Of(id), It.IsAny<CancellationToken>()))
             .ReturnsAsync(entity);

        var res = await _client.GetAsync($"/api/cities/{id}");
        var dto = await res.Content.ReadFromJsonAsync<CityDto>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.IsEnabled.Should().BeFalse();
    }
}