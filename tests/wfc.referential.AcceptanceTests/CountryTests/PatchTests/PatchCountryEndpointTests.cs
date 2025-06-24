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
using System.Linq.Expressions;

namespace wfc.referential.AcceptanceTests.CountryTests.PatchTests;

public class PatchCountryEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly Mock<ICountryRepository> _countryRepo = new();
    private readonly Mock<ICurrencyRepository> _currencyRepo = new();
    private readonly Mock<IMonetaryZoneRepository> _zoneRepo = new();
    private readonly HttpClient _client;

    public PatchCountryEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var custom = factory.WithWebHostBuilder(b =>
        {
            b.UseEnvironment("Testing");
            b.ConfigureServices(s =>
            {
                s.RemoveAll<ICountryRepository>();
                s.RemoveAll<ICurrencyRepository>();
                s.RemoveAll<IMonetaryZoneRepository>();
                s.RemoveAll<ICacheService>();

                _countryRepo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                            .Returns(Task.CompletedTask);

                s.AddSingleton(_countryRepo.Object);
                s.AddSingleton(_currencyRepo.Object);
                s.AddSingleton(_zoneRepo.Object);
                s.AddSingleton(cacheMock.Object);
            });
        });

        _client = custom.CreateClient();
    }

    private static Country MakeCountry(Guid id, string code = "AAA", bool isEnabled = true)
    {
        var mzId = MonetaryZoneId.Of(Guid.NewGuid());
        var cur = CurrencyId.Of(Guid.NewGuid());

        return Country.Create(
            CountryId.Of(id),
            abbreviation: "ABR",
            name: "Old-Name",
            code: code,
            ISO2: "AN",
            ISO3: "ANN",
            dialingCode: "+999",
            timeZone: "UTC",
            hasSector: false,
            isSmsEnabled: false,
            numberDecimalDigits: 2,
            monetaryZoneId: mzId,
            currencyId: cur);
    }

    private static async Task<HttpResponseMessage> PatchJsonAsync(
        HttpClient client, string url, object body)
    {
        var json = JsonSerializer.Serialize(body);
        var req = new HttpRequestMessage(HttpMethod.Patch, url)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        return await client.SendAsync(req);
    }

    private static async Task<bool> ReadBoolAsync(HttpResponseMessage resp)
    {
        var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();
        var root = doc!.RootElement;

        if (root.ValueKind is JsonValueKind.True or JsonValueKind.False)
            return root.GetBoolean();

        if (root.TryGetProperty("value", out var v) &&
            (v.ValueKind is JsonValueKind.True or JsonValueKind.False))
            return v.GetBoolean();

        return root.GetBoolean();
    }

    [Fact(DisplayName = "PATCH /api/countries/{id} returns 200 when patch succeeds")]
    public async Task Patch_ShouldReturn200_WhenPatchSuccessful()
    {
        // Arrange
        var id = Guid.NewGuid();
        var country = MakeCountry(id);

        _countryRepo.Setup(r => r.GetByIdAsync(CountryId.Of(id), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(country);

        _countryRepo.Setup(r => r.GetOneByConditionAsync(
                               It.IsAny<Expression<Func<Country, bool>>>(),
                               It.IsAny<CancellationToken>()))
                    .ReturnsAsync((Country?)null);     

        var payload = new
        {
            CountryId = id,
            Name = "New-Name",
            IsEnabled = false
        };

        // Act
        var resp = await PatchJsonAsync(_client, $"/api/countries/{id}", payload);
        var result = await ReadBoolAsync(resp);

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeTrue();

        country.Name.Should().Be("New-Name");
        country.IsEnabled.Should().BeFalse();
        _countryRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PATCH /api/countries/{id} returns 200 when patching only Code")]
    public async Task Patch_ShouldReturn200_WhenPatchingOnlyCode()
    {
        var id = Guid.NewGuid();
        var orig = MakeCountry(id, code: "OLD");

        _countryRepo.Setup(r => r.GetByIdAsync(CountryId.Of(id), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(orig);
        _countryRepo.Setup(r => r.GetOneByConditionAsync(
                               It.IsAny<Expression<Func<Country, bool>>>(),
                               It.IsAny<CancellationToken>()))
                    .ReturnsAsync((Country?)null);      

        var payload = new { CountryId = id, Code = "NEW" };

        var resp = await PatchJsonAsync(_client, $"/api/countries/{id}", payload);
        var ok = await ReadBoolAsync(resp);

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        ok.Should().BeTrue();

        orig.Code.Should().Be("NEW");
        _countryRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }


    [Fact(DisplayName = "PATCH /api/countries/{id} returns 404 when Country not found")]
    public async Task Patch_ShouldReturn404_WhenCountryNotFound()
    {
        var id = Guid.NewGuid();

        _countryRepo.Setup(r => r.GetByIdAsync(CountryId.Of(id), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((Country?)null);

        var payload = new { CountryId = id, Name = "Nope" };

        var resp = await PatchJsonAsync(_client, $"/api/countries/{id}", payload);

        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
        _countryRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PATCH /api/countries/{id} returns 409 when Code already exists")]
    public async Task Patch_ShouldReturn409_WhenCodeDuplicate()
    {
        var idTarget = Guid.NewGuid();
        var idExisting = Guid.NewGuid();

        var target = MakeCountry(idTarget, code: "OLD");
        var existing = MakeCountry(idExisting, code: "DUPL");

        _countryRepo.Setup(r => r.GetByIdAsync(CountryId.Of(idTarget), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(target);

        _countryRepo.Setup(r => r.GetOneByConditionAsync(
                               It.IsAny<Expression<Func<Country, bool>>>(),
                               It.IsAny<CancellationToken>()))
                    .ReturnsAsync(existing);     

        var payload = new { CountryId = idTarget, Code = "DUPL" };

        var resp = await PatchJsonAsync(_client, $"/api/countries/{idTarget}", payload);
        var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();

        resp.StatusCode.Should().Be(HttpStatusCode.Conflict);
        doc!.RootElement.GetProperty("errors")
                .GetProperty("message").GetString()
           .Should().Be("Country with code DUPL already exists.");

        _countryRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PATCH /api/countries/{id} returns 404 when CurrencyId not found")]
    public async Task Patch_ShouldReturn404_WhenCurrencyNotFound()
    {
        var id = Guid.NewGuid();
        var cId = Guid.NewGuid();
        var orig = MakeCountry(id);

        _countryRepo.Setup(r => r.GetByIdAsync(CountryId.Of(id), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(orig);
        _countryRepo.Setup(r => r.GetOneByConditionAsync(It.IsAny<Expression<Func<Country, bool>>>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((Country?)null);

        _currencyRepo.Setup(r => r.GetByIdAsync(CurrencyId.Of(cId), It.IsAny<CancellationToken>()))
                     .ReturnsAsync((Currency?)null);  

        var payload = new { CountryId = id, CurrencyId = cId };

        var resp = await PatchJsonAsync(_client, $"/api/countries/{id}", payload);

        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
        _countryRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }


    [Fact(DisplayName = "PATCH /api/countries/{id} returns 400 when Code is empty string")]
    public async Task Patch_ShouldReturn400_WhenCodeEmpty()
    {
        var id = Guid.NewGuid();
        var body = new { CountryId = id, Code = "" };

        var resp = await PatchJsonAsync(_client, $"/api/countries/{id}", body);
        var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}