using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Moq;
using wfc.referential.Domain.CityAggregate;
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.RegionAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.RegionTests.DeleteTests;

public class DeleteRegionEndpointTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{
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

        _regionRepoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<Expression<Func<Region, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(region);
        _regionRepoMock.Setup(r => r.GetByConditionAsync(It.IsAny<Expression<Func<Region, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<Region>()); // No cities

        // Act
        var response = await _client.DeleteAsync($"/api/regions/{regionId}");
        var result = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeTrue();

        _regionRepoMock.Verify(r => r.Update(It.Is<Region>(r => r.Id == RegionId.Of(regionId) && !r.IsEnabled.Equals(true))), Times.Once);
    }

    [Fact(DisplayName = "DELETE /api/regions/{id} returns 404 when region does not exist")]
    public async Task Delete_ShouldReturn404_WhenRegionDoesNotExist()
    {
        // Arrange
        var regionId = Guid.NewGuid();
        _regionRepoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<Expression<Func<Region, bool>>>(), It.IsAny<CancellationToken>()))
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

        _regionRepoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<Expression<Func<Region, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(region);
        _cityRepoMock.Setup(r => r.GetByConditionAsync(
            It.IsAny<Expression<Func<City, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cities);// Region has cities

        // Act
        var response = await _client.DeleteAsync($"/api/regions/{regionId}");
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var root = doc!.RootElement;
        root.GetProperty("title").GetString().Should().Be("Bad Request");
        root.GetProperty("status").GetInt32().Should().Be(400);
        root.GetProperty("errors").GetProperty("message").GetString().Should().StartWith("Cannot delete the region because it has associated cities.");

        // Verify that the update method was not called
        _regionRepoMock.Verify(r => r.Update(It.IsAny<Region>()), Times.Never);
    }
}
