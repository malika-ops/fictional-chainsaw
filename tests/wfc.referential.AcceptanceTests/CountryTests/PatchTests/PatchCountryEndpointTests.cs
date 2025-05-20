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
using System.Text;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.CurrencyAggregate;
using wfc.referential.Domain.MonetaryZoneAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.CountryTests.PatchTests;

public class PatchCountryEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<ICountryRepository> _countryMock = new();
    private readonly Mock<ICurrencyRepository> _currencyMock = new();

    /*───────────────────────── ctor ─────────────────────────*/
    public PatchCountryEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var customised = factory.WithWebHostBuilder(b =>
        {
            b.UseEnvironment("Testing");

            b.ConfigureServices(s =>
            {
                s.RemoveAll<ICountryRepository>();
                s.RemoveAll<ICurrencyRepository>();
                s.RemoveAll<ICacheService>();

                s.AddSingleton(_countryMock.Object);
                s.AddSingleton(_currencyMock.Object);
                s.AddSingleton(cacheMock.Object);
            });
        });

        _client = customised.CreateClient();
    }

    /*──────────────────────── helpers ─────────────────────────*/

    private static Country CountryAgg(Guid id, string tz = "UTC") =>
        Country.Create(
            CountryId.Of(id),
            "USA",                               // Abbreviation
            "United States",
            "USA", "US", "USA",
            "+1", tz,
            hasSector: false,
            isSmsEnabled: false,
            numberDecimalDigits: 2,
            isEnabled: true,
            monetaryZoneId: MonetaryZoneId.Of(Guid.NewGuid()),
            currencyId: CurrencyId.Of(Guid.NewGuid()));

    private static async Task<HttpResponseMessage> PatchJsonAsync(
        HttpClient client, string url, object body)
    {
        var json = JsonSerializer.Serialize(body);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var req = new HttpRequestMessage(HttpMethod.Patch, url) { Content = content };
        return await client.SendAsync(req);
    }

    /// <summary>
    /// Reads a Guid from FastEndpoints responses whether it is returned
    /// directly <c>"guid"</c> or wrapped <c>{ "value": "guid" }</c>.
    /// </summary>
    private static async Task<Guid> ReadGuidAsync(HttpResponseMessage resp)
    {
        var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();
        var root = doc!.RootElement;

        if (root.ValueKind == JsonValueKind.String)
            return Guid.Parse(root.GetString()!);

        if (root.TryGetProperty("value", out var v) && v.ValueKind == JsonValueKind.String)
            return Guid.Parse(v.GetString()!);

        return root.GetGuid();   // fallback (very rare)
    }

    /*──────────────────────── tests ─────────────────────────*/

    [Fact(DisplayName = "PATCH /api/countries/{id} returns 200 when patch succeeds")]
    public async Task Patch_ShouldReturn200_WhenUpdateSuccessful()
    {
        /*──── Arrange ────*/
        var id = Guid.NewGuid();
        var orig = CountryAgg(id, "UTC");

        _countryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(orig);

        Country? saved = null;
        _countryMock.Setup(r => r.UpdateAsync(It.IsAny<Country>(),
                                              It.IsAny<CancellationToken>()))
                    .Callback<Country, CancellationToken>((c, _) => saved = c)
                    .Returns(Task.CompletedTask);

        var payload = new
        {
            CountryId = id,
            Abbreviation = "USN",  // changed
            TimeZone = "EST"   // changed
            /* all other props omitted → must stay untouched */
        };

        /*──── Act ────*/
        var resp = await PatchJsonAsync(_client, $"/api/countries/{id}", payload);
        var result = await ReadGuidAsync(resp);

        /*──── Assert ────*/
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().Be(id);

        saved!.Abbreviation.Should().Be("USN");
        saved.TimeZone.Should().Be("EST");
        saved.Name.Should().Be("United States");   // not modified

        _countryMock.Verify(r => r.UpdateAsync(It.IsAny<Country>(),
                                               It.IsAny<CancellationToken>()),
                            Times.Once);
    }
}