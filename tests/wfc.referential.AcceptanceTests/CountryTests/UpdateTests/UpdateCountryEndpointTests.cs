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
using wfc.referential.Domain.MonetaryZoneAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.CountryTests.UpdateTests;

public class UpdateCountryEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<ICountryRepository> _countryRepo = new();
    private readonly Mock<ICurrencyRepository> _currencyRepo = new();

    /*──────────────────────── ctor ────────────────────────*/
    public UpdateCountryEndpointTests(WebApplicationFactory<Program> factory)
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

                s.AddSingleton(_countryRepo.Object);
                s.AddSingleton(_currencyRepo.Object);
                s.AddSingleton(cacheMock.Object);

                // no-op update by default
                _countryRepo.Setup(r => r.UpdateAsync(It.IsAny<Country>(),
                                                      It.IsAny<CancellationToken>()))
                            .Returns(Task.CompletedTask);
            });
        });

        _client = customised.CreateClient();
    }

    /*──────────────────── helpers ────────────────────*/

    private static Country MakeCountry(Guid id, string code, string name) =>
        Country.Create(
            CountryId.Of(id), "AB", name, code,
            "IS", "ISO", "+0", "UTC",
            false, false, 2, true,
            MonetaryZoneId.Of(Guid.NewGuid()),
            null);

    /// <summary>
    /// Sends a JSON body with PUT.
    /// </summary>
    private static Task<HttpResponseMessage> PutJsonAsync(
        HttpClient client, string url, object body)
    {
        var content = JsonContent.Create(body);
        return client.PutAsync(url, content);
    }

    /// <summary>
    /// Reads a Guid from responses whether FastEndpoints wraps it
    /// (<c>{ "value": "&lt;guid>" }</c>) or returns a bare string / GUID.
    /// </summary>
    private static async Task<Guid> ReadGuidAsync(HttpResponseMessage resp)
    {
        var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();
        var root = doc!.RootElement;

        if (root.ValueKind == JsonValueKind.String)
            return Guid.Parse(root.GetString()!);

        if (root.TryGetProperty("value", out var val) && val.ValueKind == JsonValueKind.String)
            return Guid.Parse(val.GetString()!);

        return root.GetGuid();           // fallback
    }

    private static string FirstError(JsonElement root, string key)
    {
        if (root.TryGetProperty("errors", out var errs))
        {
            if (errs.ValueKind == JsonValueKind.String)
                return errs.GetString()!;              // simple string

            if (errs.TryGetProperty(key, out var arr))
                return arr[0].GetString()!;
        }
        throw new InvalidOperationException("Error message not found");
    }

    /*──────────────────── tests ─────────────────────*/

    /* ----------------------------------------------------------------
       1) Happy-path update
       ----------------------------------------------------------------*/
    [Fact(DisplayName = "PUT /api/countries/{id} returns 200 when update succeeds")]
    public async Task Put_ShouldReturn200_WhenUpdateIsSuccessful()
    {
        // Arrange
        var id = Guid.NewGuid();
        var mz = Guid.NewGuid();
        var old = MakeCountry(id, "USA", "United States");

        _countryRepo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(old);

        _countryRepo.Setup(r => r.GetByCodeAsync("CAN", It.IsAny<CancellationToken>()))
                    .ReturnsAsync((Country?)null);  // code remains unique

        Country? saved = null;
        _countryRepo.Setup(r => r.UpdateAsync(It.IsAny<Country>(),
                                              It.IsAny<CancellationToken>()))
                    .Callback<Country, CancellationToken>((c, _) => saved = c);

        var payload = new
        {
            CountryId = id,
            Abbreviation = "CA",
            Name = "Canada",
            Code = "CAN",
            ISO2 = "CA",
            ISO3 = "CAN",
            DialingCode = "+1",
            TimeZone = "UTC-5",
            HasSector = false,
            IsSmsEnabled = false,
            NumberDecimalDigits = 2,
            MonetaryZoneId = mz,
            CurrencyId = Guid.NewGuid()
        };

        // Act
        var resp = await PutJsonAsync(_client, $"/api/countries/{id}", payload);
        var result = await ReadGuidAsync(resp);

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().Be(id);

        saved!.Name.Should().Be("Canada");
        saved.Code.Should().Be("CAN");
        saved.Abbreviation.Should().Be("CA");
        saved.MonetaryZoneId.Value.Should().Be(mz);

        _countryRepo.Verify(r => r.UpdateAsync(It.IsAny<Country>(),
                                               It.IsAny<CancellationToken>()),
                            Times.Once);
    }

    /* ----------------------------------------------------------------
       2) Validation error – Name missing
       ----------------------------------------------------------------*/
    [Fact(DisplayName = "PUT /api/countries/{id} returns 400 when Name is missing")]
    public async Task Put_ShouldReturn400_WhenNameMissing()
    {
        // Arrange
        var id = Guid.NewGuid();
        var payload = new
        {
            CountryId = id,
            Abbreviation = "US",
            Code = "USA",
            ISO2 = "US",
            ISO3 = "USA",
            DialingCode = "+1",
            TimeZone = "UTC-5",
            HasSector = false,
            IsSmsEnabled = false,
            NumberDecimalDigits = 2,
            MonetaryZoneId = Guid.NewGuid(),
            CurrencyId = Guid.NewGuid()
            // Name omitted
        };

        // Act
        var resp = await PutJsonAsync(_client, $"/api/countries/{id}", payload);
        var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        FirstError(doc!.RootElement, "name")
            .Should().Be("Name is required.");

        _countryRepo.Verify(r => r.UpdateAsync(It.IsAny<Country>(),
                                               It.IsAny<CancellationToken>()),
                            Times.Never);
    }

    /* ----------------------------------------------------------------
       3) Duplicate Code
       ----------------------------------------------------------------*/
    [Fact(DisplayName = "PUT /api/countries/{id} returns 400 when new code already exists")]
    public async Task Put_ShouldReturn400_WhenCodeAlreadyExists()
    {
        // Arrange
        var id = Guid.NewGuid();

        var duplicate = MakeCountry(Guid.NewGuid(), "FRA", "France");
        var target = MakeCountry(id, "USA", "United States");

        _countryRepo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(target);

        _countryRepo.Setup(r => r.GetByCodeAsync("FRA", It.IsAny<CancellationToken>()))
                    .ReturnsAsync(duplicate);          // same code elsewhere

        var payload = new
        {
            CountryId = id,
            Abbreviation = "FR",
            Name = "France",
            Code = "FRA",
            ISO2 = "FR",
            ISO3 = "FRA",
            DialingCode = "+33",
            TimeZone = "UTC+1",
            HasSector = false,
            IsSmsEnabled = false,
            NumberDecimalDigits = 2,
            MonetaryZoneId = Guid.NewGuid(),
            CurrencyId = Guid.NewGuid()
        };

        // Act
        var resp = await PutJsonAsync(_client, $"/api/countries/{id}", payload);
        var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        FirstError(doc!.RootElement, "Code")
            .Should().Be("Country with code FRA already exists.");

        _countryRepo.Verify(r => r.UpdateAsync(It.IsAny<Country>(),
                                               It.IsAny<CancellationToken>()),
                            Times.Never);
    }
}
