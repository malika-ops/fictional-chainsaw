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

namespace wfc.referential.AcceptanceTests.CountryTests.DeleteTests;

public class DeleteCountryEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<ICountryRepository> _repoMock = new();

    // ───────────────────────── constructor ─────────────────────────
    public DeleteCountryEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var customisedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<ICountryRepository>();
                services.RemoveAll<ICacheService>();

                _repoMock
                    .Setup(r => r.UpdateAsync(It.IsAny<Country>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);     // noop

                services.AddSingleton(_repoMock.Object);
                services.AddSingleton(cacheMock.Object);
            });
        });

        _client = customisedFactory.CreateClient();
    }

    // helper to create a Country quickly
    private static Country MakeCountry(Guid id, string code) =>
        Country.Create(
            CountryId.Of(id),
            "AB",
            "Name",
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

    // ────────────────────────────────────────────────────────────────
    // 1) Happy‑path: disable succeeds
    // ────────────────────────────────────────────────────────────────
    [Fact(DisplayName = "DELETE /api/countries/{id} returns 200 when country has no regions")]
    public async Task Delete_ShouldReturn200_WhenCountryHasNoRegions()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity = MakeCountry(id, "USA");     // Regions list empty

        _repoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(entity);

        Country? saved = null;
        _repoMock.Setup(r => r.UpdateAsync(It.IsAny<Country>(), It.IsAny<CancellationToken>()))
                 .Callback<Country, CancellationToken>((c, _) => saved = c)
                 .Returns(Task.CompletedTask);

        // Act
        var response = await _client.DeleteAsync($"/api/countries/{id}");
        var body = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body.Should().BeTrue();

        saved!.IsEnabled.Should().Be(false);

        _repoMock.Verify(r => r.UpdateAsync(It.IsAny<Country>(),
                                            It.IsAny<CancellationToken>()),
                         Times.Once);
    }

    // ────────────────────────────────────────────────────────────────
    // 2) Country not found
    // ────────────────────────────────────────────────────────────────
    [Fact(DisplayName = "DELETE /api/countries/{id} returns 400 when country not found")]
    public async Task Delete_ShouldReturn400_WhenCountryNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();

        _repoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Country?)null);

        // Act
        var response = await _client.DeleteAsync($"/api/countries/{id}");
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors").GetString()
           .Should().Be($"Country with ID [{id}] not found.");

        _repoMock.Verify(r => r.UpdateAsync(It.IsAny<Country>(),
                                            It.IsAny<CancellationToken>()),
                         Times.Never);
    }

    // ────────────────────────────────────────────────────────────────
    // 3) Country has regions
    // ────────────────────────────────────────────────────────────────
    [Fact(DisplayName = "DELETE /api/countries/{id} returns 400 when country has regions")]
    public async Task Delete_ShouldReturn400_WhenCountryHasRegions()
    {
        // Arrange
        var id = Guid.NewGuid();
        var withReg = MakeCountry(id, "CAN");

        // 🌱 fake one region to trigger the guard (null entry is fine for Count)
        withReg.Regions.Add(null!);

        _repoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(withReg);

        // Act
        var response = await _client.DeleteAsync($"/api/countries/{id}");
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors").GetString()
           .Should().Be("Cannot delete Country with existing regions.");

        _repoMock.Verify(r => r.UpdateAsync(It.IsAny<Country>(),
                                            It.IsAny<CancellationToken>()),
                         Times.Never);
    }
}