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
    private readonly Mock<ICountryRepository> _countryRepoMock = new();
    private readonly Mock<ICurrencyRepository> _currencyRepoMock = new();

    // constructor
    public UpdateCountryEndpointTests(WebApplicationFactory<Program> factory)
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

                // default noop for Update
                _countryRepoMock
                    .Setup(r => r.UpdateAsync(It.IsAny<Country>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

                services.AddSingleton(_countryRepoMock.Object);
                services.AddSingleton(_currencyRepoMock.Object);
                services.AddSingleton(cacheMock.Object);
            });
        });

        _client = customisedFactory.CreateClient();
    }

    // helper to build a Country quickly
    private static Country MakeCountry(Guid id, string code, string name) =>
        Country.Create(
            CountryId.Of(id),
            "AB",
            name,
            code,
            "IS2",
            "IS3",
            "+0",
            "UTC",
            false,
            false,
            2,
            true,
            MonetaryZoneId.Of(Guid.NewGuid()),
            null);

    // 1) Happy‑path update
    [Fact(DisplayName = "PUT /api/countries/{id} returns 200 when update succeeds")]
    public async Task Put_ShouldReturn200_WhenUpdateIsSuccessful()
    {
        // Arrange
        var id = Guid.NewGuid();
        var mzId = Guid.NewGuid();
        var old = MakeCountry(id, "USA", "United States");

        _countryRepoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(old);

        _countryRepoMock.Setup(r => r.GetByCodeAsync("CAN", It.IsAny<CancellationToken>()))
                        .ReturnsAsync((Country?)null); // code unique

        Country? updated = null;
        _countryRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Country>(),
                                                  It.IsAny<CancellationToken>()))
                        .Callback<Country, CancellationToken>((c, _) => updated = c)
                        .Returns(Task.CompletedTask);

        var payload = new
        {
            CountryId = id,
            Abbreviation = "CA",
            Name = "Canada",
            Code = "CAN",
            ISO2 = "CA",
            ISO3 = "CAN",
            DialingCode = "+1",
            TimeZone = "UTC‑5",
            MonetaryZoneId = mzId
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/countries/{id}", payload);
        var returned = await response.Content.ReadFromJsonAsync<Guid>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returned.Should().Be(id);

        updated!.Name.Should().Be("Canada");
        updated.Code.Should().Be("CAN");
        updated.Abbreviation.Should().Be("CA");
        updated.MonetaryZoneId.Value.Should().Be(mzId);

        _countryRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Country>(),
                                                   It.IsAny<CancellationToken>()),
                                Times.Once);
    }

    // 2) Validation error – Name missing
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
            TimeZone = "UTC‑5",
            MonetaryZoneId = Guid.NewGuid()
            // Name omitted
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/countries/{id}", payload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors")
            .GetProperty("name")[0].GetString()
            .Should().Be("Name is required.");

        _countryRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Country>(),
                                                   It.IsAny<CancellationToken>()),
                                Times.Never);
    }

    // 3) Duplicate code
    [Fact(DisplayName = "PUT /api/countries/{id} returns 400 when new code already exists")]
    public async Task Put_ShouldReturn400_WhenCodeAlreadyExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var duplicate = MakeCountry(Guid.NewGuid(), "FRA", "France");
        var target = MakeCountry(id, "USA", "United States");

        _countryRepoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(target);

        _countryRepoMock.Setup(r => r.GetByCodeAsync("FRA", It.IsAny<CancellationToken>()))
                        .ReturnsAsync(duplicate); // duplicate code

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
            MonetaryZoneId = Guid.NewGuid()
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/countries/{id}", payload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors").GetString()
           .Should().Be("Country with code FRA already exists.");

        _countryRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Country>(),
                                                   It.IsAny<CancellationToken>()),
                                Times.Never);
    }
}
