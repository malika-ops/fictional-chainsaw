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
using wfc.referential.Domain.CountryIdentityDocAggregate;
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.IdentityDocumentAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.CountryIdentityDocTests.GetByIdTests;

public class GetCountryIdentityDocByIdEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<ICountryIdentityDocRepository> _repo = new();

    public GetCountryIdentityDocByIdEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var custom = factory.WithWebHostBuilder(b =>
        {
            b.UseEnvironment("Testing");

            b.ConfigureServices(s =>
            {
                s.RemoveAll<ICountryIdentityDocRepository>();
                s.RemoveAll<ICacheService>();

                s.AddSingleton(_repo.Object);
                s.AddSingleton(cacheMock.Object);
            });
        });

        _client = custom.CreateClient();
    }

    private static CountryIdentityDoc Make(Guid id, string code = "COUNTRY-ID-DOC-001", string? name = null, bool enabled = true)
    {
        var countryIdentityDoc = CountryIdentityDoc.Create(
            id: CountryIdentityDocId.Of(id),
            countryId: CountryId.Of(Guid.NewGuid()),
            identityDocumentId: IdentityDocumentId.Of(Guid.NewGuid())
        );

        if (!enabled)
            countryIdentityDoc.Disable();

        return countryIdentityDoc;
    }

    private record CountryIdentityDocDto(Guid Id, Guid CountryId, Guid IdentityDocumentId, bool IsEnabled);

    [Fact(DisplayName = "GET /api/countryidentitydocs/{id} → 404 when CountryIdentityDoc not found")]
    public async Task Get_ShouldReturn404_WhenCountryIdentityDocNotFound()
    {
        var id = Guid.NewGuid();

        _repo.Setup(r => r.GetByIdAsync(CountryIdentityDocId.Of(id), It.IsAny<CancellationToken>()))
             .ReturnsAsync((CountryIdentityDoc?)null);

        var res = await _client.GetAsync($"/api/countryidentitydocs/{id}");
        var doc = await res.Content.ReadFromJsonAsync<JsonDocument>();

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var root = doc!.RootElement;
        root.GetProperty("title").GetString().Should().Be("Resource Not Found");
        root.GetProperty("status").GetInt32().Should().Be(404);

        _repo.Verify(r => r.GetByIdAsync(CountryIdentityDocId.Of(id), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "GET /api/countryidentitydocs/{id} → 404 when id is malformed")]
    public async Task Get_ShouldReturn404_WhenIdMalformed()
    {
        const string bad = "not-a-guid";

        var res = await _client.GetAsync($"/api/countryidentitydocs/{bad}");

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);

        _repo.Verify(r => r.GetByIdAsync(It.IsAny<CountryIdentityDocId>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "GET /api/countryidentitydocs/{id} → 200 for disabled CountryIdentityDoc")]
    public async Task Get_ShouldReturn200_WhenCountryIdentityDocDisabled()
    {
        var id = Guid.NewGuid();
        var entity = Make(id, "COUNTRY-ID-DOC-DIS", enabled: false);

        _repo.Setup(r => r.GetByIdAsync(CountryIdentityDocId.Of(id), It.IsAny<CancellationToken>()))
             .ReturnsAsync(entity);

        var res = await _client.GetAsync($"/api/countryidentitydocs/{id}");
        var dto = await res.Content.ReadFromJsonAsync<CountryIdentityDocDto>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.IsEnabled.Should().BeFalse();
    }
}