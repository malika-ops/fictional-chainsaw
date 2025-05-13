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

namespace wfc.referential.AcceptanceTests.CountryTests.CreateTests;

public class CreateCountryEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<ICountryRepository> _countryRepoMock = new();
    private readonly Mock<ICurrencyRepository> _currencyRepoMock = new();

    // constructor
    public CreateCountryEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var customisedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<ICountryRepository>();
                services.RemoveAll<ICurrencyRepository>();
                services.RemoveAll<ICacheService>();

                // default behaviour for AddAsync – echo back entity
                _countryRepoMock
                    .Setup(r => r.AddAsync(It.IsAny<Country>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((Country c, CancellationToken _) => c);

                services.AddSingleton(_countryRepoMock.Object);
                services.AddSingleton(_currencyRepoMock.Object);
                services.AddSingleton(cacheMock.Object);
            });
        });

        _client = customisedFactory.CreateClient();
    }

    // helper to build a Country quickly (for duplicate‑code test)
    private static Country MakeCountry(string code) =>
        Country.Create(
            CountryId.Of(Guid.NewGuid()),
            "US",              // abbreviation
            "United States",
            code,
            "US", "USA", "+1", "UTC‑5", false, false, 2,
            true,
            MonetaryZoneId.Of(Guid.NewGuid()),
            null);

    // 1) Happy‑path creation
    [Fact(DisplayName = "POST /api/countries returns 200 and Guid when payload is valid")]
    public async Task Post_ShouldReturn200_WhenRequestIsValid()
    {
        // Arrange
        var mzId = Guid.NewGuid();
        var payload = new
        {
            Abbreviation = "US",
            Name = "United States",
            Code = "USA",
            ISO2 = "US",
            ISO3 = "USA",
            DialingCode = "+1",
            TimeZone = "UTC‑5",
            MonetaryZoneId = mzId
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/countries", payload);
        var returnedId = await response.Content.ReadFromJsonAsync<Guid>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returnedId.Should().NotBeEmpty();

        _countryRepoMock.Verify(r =>
            r.AddAsync(It.Is<Country>(c =>
                    c.Abbreviation == payload.Abbreviation &&
                    c.Name == payload.Name &&
                    c.Code == payload.Code &&
                    c.ISO2 == payload.ISO2 &&
                    c.ISO3 == payload.ISO3 &&
                    c.DialingCode == payload.DialingCode &&
                    c.TimeZone == payload.TimeZone &&
                    c.MonetaryZoneId.Value == mzId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // 2) Validation error – Name missing
    [Fact(DisplayName = "POST /api/countries returns 400 when Name is missing")]
    public async Task Post_ShouldReturn400_WhenNameMissing()
    {
        // Arrange
        var payload = new
        {
            Abbreviation = "US",
            // Name omitted
            Code = "USA",
            ISO2 = "US",
            ISO3 = "USA",
            DialingCode = "+1",
            TimeZone = "UTC‑5",
            MonetaryZoneId = Guid.NewGuid()
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/countries", payload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors")
            .GetProperty("name")[0].GetString()
            .Should().Be("Country name is required.");

        _countryRepoMock.Verify(r =>
            r.AddAsync(It.IsAny<Country>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // 3) Duplicate code
    [Fact(DisplayName = "POST /api/countries returns 400 when Code already exists")]
    public async Task Post_ShouldReturn400_WhenCodeAlreadyExists()
    {
        // Arrange
        const string duplicateCode = "FRA";

        _countryRepoMock
            .Setup(r => r.GetByCodeAsync(duplicateCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeCountry(duplicateCode));   // simulate existing country

        var payload = new
        {
            Abbreviation = "FR",
            Name = "France",
            Code = duplicateCode,
            ISO2 = "FR",
            ISO3 = "FRA",
            DialingCode = "+33",
            TimeZone = "UTC+1",
            MonetaryZoneId = Guid.NewGuid()
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/countries", payload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors").GetString()
           .Should().Be($"Country with code {duplicateCode} already exists.");

        _countryRepoMock.Verify(r =>
            r.AddAsync(It.IsAny<Country>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
