using BuildingBlocks.Application.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using System.Net;
using System.Net.Http.Json;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.MonetaryZoneAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.CountryTests.PatchTests;

public class PatchCountryEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<ICountryRepository> _countryMock = new();
    private readonly Mock<ICurrencyRepository> _currencyMock = new();

    // ───────────────────────── ctor ─────────────────────────
    public PatchCountryEndpointTests(WebApplicationFactory<Program> factory)
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

                services.AddSingleton(_countryMock.Object);
                services.AddSingleton(_currencyMock.Object);
                services.AddSingleton(cacheMock.Object);
            });
        });

        _client = customisedFactory.CreateClient();
    }

    // helper to spin‑up an aggregate quickly
    private static Country CountryAgg(Guid id, string tz = "UTC") =>
        Country.Create(
            CountryId.Of(id), "USA", "United States", "USA", "US", "USA",
            "+1", tz, false, false, 2, true, MonetaryZoneId.Of(Guid.NewGuid()), null);

    // 1) Happy path – partial update
    [Fact(DisplayName = "PATCH /api/countries/{id} returns 200 when patch succeeds")]
    public async Task Patch_ShouldReturn200_WhenUpdateSuccessful()
    {
        // Arrange
        var id = Guid.NewGuid();
        var orig = CountryAgg(id, "UTC");

        _countryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(orig);

        Country? patched = null;
        _countryMock.Setup(r => r.UpdateAsync(It.IsAny<Country>(),
                                              It.IsAny<CancellationToken>()))
                    .Callback<Country, CancellationToken>((c, _) => patched = c)
                    .Returns(Task.CompletedTask);

        var payload = new
        {
            CountryId = id,
            Abbreviation = "USN",     // changed
            TimeZone = "EST"      // changed
            // everything else left null ⇒ unchanged
        };

        // Act
        var response = await _client.PatchAsJsonAsync($"/api/countries/{id}", payload);
        var returned = await response.Content.ReadFromJsonAsync<Guid>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returned.Should().Be(id);

        patched!.Abbreviation.Should().Be("USN");
        patched.TimeZone.Should().Be("EST");
        patched.Name.Should().Be("United States");   // NOT touched

        _countryMock.Verify(r => r.UpdateAsync(It.IsAny<Country>(),
                                               It.IsAny<CancellationToken>()),
                            Times.Once);
    }
}