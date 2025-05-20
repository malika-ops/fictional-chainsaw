using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Abstraction.Domain;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.CityAggregate;
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.RegionAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.RegionTests.DeleteTests;

public class DeleteRegionEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IRegionRepository> _repoMock = new();

    public DeleteRegionEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        // Clone the factory and customize the host
        var customisedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                // 🧹 Remove concrete registrations that hit the DB / Redis
                services.RemoveAll<IRegionRepository>();
                services.RemoveAll<ICacheService>();

                // 🔌 Plug mocks back in
                services.AddSingleton(_repoMock.Object);
                services.AddSingleton(cacheMock.Object);
            });
        });

        _client = customisedFactory.CreateClient();
    }

    [Fact(DisplayName = "DELETE /api/regions/{id} returns true when region is deleted successfully")]
    public async Task Delete_ShouldReturnTrue_WhenRegionExistsAndHasNoCities()
    {
        // Arrange
        var regionId = Guid.NewGuid();
        var region = Region.Create(
            RegionId.Of(regionId),
            "01",
            "testAAB",
            CountryId.Of(Guid.NewGuid()));

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<Guid>(id => id == regionId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(region);
        _repoMock.Setup(r => r.GetCitiesByRegionIdAsync(It.Is<Guid>(id => id == regionId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<City>()); // No cities

        // Act
        var response = await _client.DeleteAsync($"/api/regions/{regionId}");
        var result = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeTrue();

        _repoMock.Verify(r => r.UpdateRegionAsync(It.Is<Region>(r => r.Id == RegionId.Of(regionId) && !r.IsEnabled.Equals(true)), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "DELETE /api/regions/{id} returns 404 when region does not exist")]
    public async Task Delete_ShouldReturn404_WhenRegionDoesNotExist()
    {
        // Arrange
        var regionId = Guid.NewGuid();
        _repoMock.Setup(r => r.GetByIdAsync(regionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Region)null); // Region not found

        // Act
        var response = await _client.DeleteAsync($"/api/regions/{regionId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "DELETE /api/regions/{id} returns 409 when region has associated cities")]
    public async Task Delete_ShouldReturn409_WhenRegionHasCities()
    {
        // Arrange
        var regionId = Guid.Parse("174547a5-4738-42b8-90e3-cdb574ec94d4");
        var region = Region.Create(RegionId.Of(regionId), "code", "name", CountryId.Of(Guid.NewGuid()));
        var cities = new List<City>
        {
            City.Create(CityId.Of(Guid.NewGuid()), "code", "name", "timezone", region.Id, "abbreviation")
        };

        _repoMock.Setup(r => r.GetByIdAsync(regionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(region);
        _repoMock.Setup(r => r.GetCitiesByRegionIdAsync(regionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cities); // Region has cities

        // Act
        var response = await _client.DeleteAsync($"/api/regions/{regionId}");
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var root = doc!.RootElement;
        root.GetProperty("title").GetString().Should().Be("Bad Request");
        root.GetProperty("status").GetInt32().Should().Be(400);
        root.GetProperty("errors").GetString().Should().StartWith("Cannot delete the region because it has associated cities.");

        // Verify that the update method was not called
        _repoMock.Verify(r => r.UpdateRegionAsync(It.IsAny<Region>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
