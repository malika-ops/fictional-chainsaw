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
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.CurrencyAggregate;
using wfc.referential.Domain.MonetaryZoneAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.CountryTests.GetByIdTests;

public class GetCountryByIdEndpointTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{
    private static Country Make(Guid id, string code = "US", string? name = null, bool enabled = true)
    {
        var country = Country.Create(
            id: CountryId.Of(id),
            abbreviation: code.Substring(0, Math.Min(2, code.Length)),
            name: name ?? $"Country-{code}",
            code: code,
            ISO2: code.PadRight(2, 'X').Substring(0, 2),
            ISO3: code.PadRight(3, 'X').Substring(0, 3),
            dialingCode: "+1",
            timeZone: "UTC",
            hasSector: false,
            isSmsEnabled: true,
            numberDecimalDigits: 2,
            monetaryZoneId: MonetaryZoneId.Of(Guid.NewGuid()),
            currencyId: CurrencyId.Of(Guid.NewGuid())
        );

        if (!enabled)
            country.Disable();

        return country;
    }

    private record CountryDto(Guid Id, string Code, string Name, bool IsEnabled);

    [Fact(DisplayName = "GET /api/countries/{id} → 200 when Country exists")]
    public async Task Get_ShouldReturn200_WhenCountryExists()
    {
        var id = Guid.NewGuid();
        var entity = Make(id, "US", "United States");

        _countryRepoMock.Setup(r => r.GetByIdAsync(CountryId.Of(id), It.IsAny<CancellationToken>()))
             .ReturnsAsync(entity);

        var res = await _client.GetAsync($"/api/countries/{id}");
        var body = await res.Content.ReadFromJsonAsync<CountryDto>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        body!.Id.Should().Be(id);
        body.Code.Should().Be("US");
        body.Name.Should().Be("United States");
        body.IsEnabled.Should().BeTrue();

        _countryRepoMock.Verify(r => r.GetByIdAsync(CountryId.Of(id), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "GET /api/countries/{id} → 404 when Country not found")]
    public async Task Get_ShouldReturn404_WhenCountryNotFound()
    {
        var id = Guid.NewGuid();

        _countryRepoMock.Setup(r => r.GetByIdAsync(CountryId.Of(id), It.IsAny<CancellationToken>()))
             .ReturnsAsync((Country?)null);

        var res = await _client.GetAsync($"/api/countries/{id}");
        var doc = await res.Content.ReadFromJsonAsync<JsonDocument>();

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var root = doc!.RootElement;
        root.GetProperty("title").GetString().Should().Be("Resource Not Found");
        root.GetProperty("status").GetInt32().Should().Be(404);

        _countryRepoMock.Verify(r => r.GetByIdAsync(CountryId.Of(id), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "GET /api/countries/{id} → 404 when id is malformed")]
    public async Task Get_ShouldReturn404_WhenIdMalformed()
    {
        const string bad = "not-a-guid";

        var res = await _client.GetAsync($"/api/countries/{bad}");

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);

        _countryRepoMock.Verify(r => r.GetByIdAsync(It.IsAny<CountryId>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "GET /api/countries/{id} → 200 for disabled Country")]
    public async Task Get_ShouldReturn200_WhenCountryDisabled()
    {
        var id = Guid.NewGuid();
        var entity = Make(id, "XX", enabled: false);

        _countryRepoMock.Setup(r => r.GetByIdAsync(CountryId.Of(id), It.IsAny<CancellationToken>()))
             .ReturnsAsync(entity);

        var res = await _client.GetAsync($"/api/countries/{id}");
        var dto = await res.Content.ReadFromJsonAsync<CountryDto>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.IsEnabled.Should().BeFalse();
    }
}