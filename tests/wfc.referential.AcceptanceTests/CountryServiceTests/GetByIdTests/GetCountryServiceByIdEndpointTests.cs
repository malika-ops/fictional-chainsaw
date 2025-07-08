using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Moq;
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.CountryServiceAggregate;
using wfc.referential.Domain.ServiceAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.CountryServiceTests.GetByIdTests;

public class GetCountryServiceByIdEndpointTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{
    private static CountryService Make(Guid id, string code = "COUNTRY-SERVICE-001", string? name = null, bool enabled = true)
    {
        var countryService = CountryService.Create(
            id: CountryServiceId.Of(id),
            countryId: CountryId.Of(Guid.NewGuid()),
            serviceId: ServiceId.Of(Guid.NewGuid())
        );

        if (!enabled)
            countryService.Disable();

        return countryService;
    }

    private record CountryServiceDto(Guid Id, Guid CountryId, Guid ServiceId, bool IsEnabled);

    [Fact(DisplayName = "GET /api/country-services/{id} → 404 when CountryService not found")]
    public async Task Get_ShouldReturn404_WhenCountryServiceNotFound()
    {
        var id = Guid.NewGuid();

        _countryServiceRepoMock.Setup(r => r.GetByIdAsync(CountryServiceId.Of(id), It.IsAny<CancellationToken>()))
             .ReturnsAsync((CountryService?)null);

        var res = await _client.GetAsync($"/api/country-services/{id}");
        var doc = await res.Content.ReadFromJsonAsync<JsonDocument>();

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var root = doc!.RootElement;
        root.GetProperty("title").GetString().Should().Be("Resource Not Found");
        root.GetProperty("status").GetInt32().Should().Be(404);

        _countryServiceRepoMock.Verify(r => r.GetByIdAsync(CountryServiceId.Of(id), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "GET /api/country-services/{id} → 404 when id is malformed")]
    public async Task Get_ShouldReturn404_WhenIdMalformed()
    {
        const string bad = "not-a-guid";

        var res = await _client.GetAsync($"/api/country-services/{bad}");

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);

        _countryServiceRepoMock.Verify(r => r.GetByIdAsync(It.IsAny<CountryServiceId>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "GET /api/country-services/{id} → 200 for disabled CountryService")]
    public async Task Get_ShouldReturn200_WhenCountryServiceDisabled()
    {
        var id = Guid.NewGuid();
        var entity = Make(id, "COUNTRY-SERVICE-DIS", enabled: false);

        _countryServiceRepoMock.Setup(r => r.GetByIdAsync(CountryServiceId.Of(id), It.IsAny<CancellationToken>()))
             .ReturnsAsync(entity);

        var res = await _client.GetAsync($"/api/country-services/{id}");
        var dto = await res.Content.ReadFromJsonAsync<CountryServiceDto>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.IsEnabled.Should().BeFalse();
    }
}