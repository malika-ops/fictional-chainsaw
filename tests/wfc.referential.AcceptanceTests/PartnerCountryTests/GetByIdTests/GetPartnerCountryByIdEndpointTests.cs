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
using wfc.referential.Domain.PartnerCountryAggregate;
using wfc.referential.Domain.PartnerAggregate;
using wfc.referential.Domain.Countries;
using Xunit;

namespace wfc.referential.AcceptanceTests.PartnerCountryTests.GetByIdTests;

public class GetPartnerCountryByIdEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IPartnerCountryRepository> _repo = new();

    public GetPartnerCountryByIdEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var custom = factory.WithWebHostBuilder(b =>
        {
            b.UseEnvironment("Testing");

            b.ConfigureServices(s =>
            {
                s.RemoveAll<IPartnerCountryRepository>();
                s.RemoveAll<ICacheService>();

                s.AddSingleton(_repo.Object);
                s.AddSingleton(cacheMock.Object);
            });
        });

        _client = custom.CreateClient();
    }

    private static PartnerCountry Make(Guid id, string code = "PARTNER-COUNTRY-001", string? name = null, bool enabled = true)
    {
        var partnerCountry = PartnerCountry.Create(
            id: PartnerCountryId.Of(id),
            partnerId: PartnerId.Of(Guid.NewGuid()),
            countryId: CountryId.Of(Guid.NewGuid())
        );

        if (!enabled)
            partnerCountry.Disable();

        return partnerCountry;
    }

    private record PartnerCountryDto(Guid Id, Guid PartnerId, Guid CountryId, bool IsEnabled);

    [Fact(DisplayName = "GET /api/partner-countries/{id} → 404 when PartnerCountry not found")]
    public async Task Get_ShouldReturn404_WhenPartnerCountryNotFound()
    {
        var id = Guid.NewGuid();

        _repo.Setup(r => r.GetByIdAsync(PartnerCountryId.Of(id), It.IsAny<CancellationToken>()))
             .ReturnsAsync((PartnerCountry?)null);

        var res = await _client.GetAsync($"/api/partner-countries/{id}");
        var doc = await res.Content.ReadFromJsonAsync<JsonDocument>();

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var root = doc!.RootElement;
        root.GetProperty("title").GetString().Should().Be("Resource Not Found");
        root.GetProperty("status").GetInt32().Should().Be(404);

        _repo.Verify(r => r.GetByIdAsync(PartnerCountryId.Of(id), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "GET /api/partner-countries/{id} → 404 when id is malformed")]
    public async Task Get_ShouldReturn404_WhenIdMalformed()
    {
        const string bad = "not-a-guid";

        var res = await _client.GetAsync($"/api/partner-countries/{bad}");

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);

        _repo.Verify(r => r.GetByIdAsync(It.IsAny<PartnerCountryId>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "GET /api/partner-countries/{id} → 200 for disabled PartnerCountry")]
    public async Task Get_ShouldReturn200_WhenPartnerCountryDisabled()
    {
        var id = Guid.NewGuid();
        var entity = Make(id, "PARTNER-COUNTRY-DIS", enabled: false);

        _repo.Setup(r => r.GetByIdAsync(PartnerCountryId.Of(id), It.IsAny<CancellationToken>()))
             .ReturnsAsync(entity);

        var res = await _client.GetAsync($"/api/partner-countries/{id}");
        var dto = await res.Content.ReadFromJsonAsync<PartnerCountryDto>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.IsEnabled.Should().BeFalse();
    }
}