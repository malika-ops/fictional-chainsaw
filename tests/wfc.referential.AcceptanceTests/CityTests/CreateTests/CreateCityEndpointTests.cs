using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Moq;
using wfc.referential.Application.Cities.Dtos;
using wfc.referential.Application.Constants;
using wfc.referential.Domain.CityAggregate;
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.RegionAggregate;
using Xunit;


namespace wfc.referential.AcceptanceTests.CityTests.CreateTests;

public class CreateCityEndpointTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{
    [Fact(DisplayName = "POST /api/cities returns 200 and Guid (fixture version)")]
    public async Task Post_ShouldReturn200_AndId_WhenRequestIsValid()
    {
        // Arrange
        var regionId = Guid.Parse("a3b8b953-2489-49c5-aac4-c97df06d5060");
        var payload = new CreateCityRequest
        {
            CityCode= "NYC",
            CityName= "New York City",
            Abbreviation= "NYC",
            RegionId= regionId,
            TimeZone= "America/New_York"
        };
        var region = Region.Create(RegionId.Of(regionId), "RegionCode", "RegionName", CountryId.Of(Guid.NewGuid()));
        _cityRepoMock
             .Setup(r => r.GetOneByConditionAsync(It.IsAny<Expression<Func<City, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync((City)null);

        _regionRepoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<Expression<Func<Region, bool>>>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(region);
        // Act
        var response = await _client.PostAsJsonAsync("/api/cities", payload);
        var returnedId = await response.Content.ReadFromJsonAsync<Guid>();

        // Assert (FluentAssertions)
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returnedId.Should().NotBeEmpty();

        // verify repository interaction using *FluentAssertions on Moq invocations
        _cityRepoMock.Verify(r =>
            r.AddAsync(It.Is<City>(r =>
                    r.Code == payload.CityCode &&
                    r.Name == payload.CityName),
                    It.IsAny<CancellationToken>()
            ),
            Times.Once
        );


        _cacheMock.Verify(c => c.RemoveByPrefixAsync(CacheKeys.City.Prefix, It.IsAny<CancellationToken>()), Times.Once);

    }

    [Fact(DisplayName = "POST /api/cities returns 400 & problem‑details when Code is missing")]
    public async Task Post_ShouldReturn400_WhenValidationFails()
    {
        var invalidPayload = new CreateCityRequest
        {
            CityName = "New York City",
            Abbreviation = "NYC",
            RegionId = Guid.Parse("a3b8b953-2489-49c5-aac4-c97df06d5060"),
            TimeZone = "America/New_York"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/cities", invalidPayload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var root = doc!.RootElement;
        root.GetProperty("title").GetString().Should().Be("One or more validation errors occurred.");
        root.GetProperty("status").GetInt32().Should().Be(400);

        root.GetProperty("errors")
            .GetProperty("CityCode")[0].GetString()
            .Should().Be("Code is required");

        // the handler must NOT be reached
        _cityRepoMock.Verify(r =>
            r.AddAsync(It.IsAny<City>(), It.IsAny<CancellationToken>()),
            Times.Never,
            "when validation fails, the command handler should not be executed");
    }

    [Fact(DisplayName = "POST /api/cities returns 400 & problem‑details when Name and Code are missing")]
    public async Task Post_ShouldReturn400_WhenNameAndCodeAreMissing()
    {
        // ---------- Arrange ----------
        var invalidPayload = new CreateCityRequest
        {
            Abbreviation = "NYC",
            RegionId = Guid.Parse("a3b8b953-2489-49c5-aac4-c97df06d5060"),
            TimeZone = "America/New_York"
        };

        // ---------- Act ----------
        var response = await _client.PostAsJsonAsync("/api/cities", invalidPayload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // ---------- Assert ----------
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var root = doc!.RootElement;

        root.GetProperty("title").GetString().Should().Be("One or more validation errors occurred.");
        root.GetProperty("status").GetInt32().Should().Be(400);

        var errors = root.GetProperty("errors");

        errors.GetProperty("CityName")[0].GetString()
              .Should().Be("Name is required");

        errors.GetProperty("CityCode")[0].GetString()
              .Should().Be("Code is required");

        // handler must NOT run on validation failure
        _cityRepoMock.Verify(r =>
            r.AddAsync(It.IsAny<City>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact(DisplayName = "POST /api/cities returns 400 when Code already exists")]
    public async Task Post_ShouldReturn409_WhenCodeAlreadyExists()
    {
        // Arrange 
        const string duplicateCode = "99";

        // Tell the repo mock that the code already exists
        var city = City.Create(CityId.Of(Guid.NewGuid()), "", "", "", RegionId.Of(Guid.NewGuid()), "");

        _cityRepoMock
            .Setup(r => r.GetOneByConditionAsync(It.IsAny<Expression<Func<City, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(city);

        var payload = new CreateCityRequest
        {
            CityCode = duplicateCode,
            CityName = "New York City",
            Abbreviation = "NYC",
            RegionId = Guid.Parse("a3b8b953-2489-49c5-aac4-c97df06d5060"),
            TimeZone = "America/New_York"
        }; 

        // Act
        var response = await _client.PostAsJsonAsync("/api/cities", payload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var root = doc!.RootElement;
        var error = root.GetProperty("errors")
                .GetProperty("message").GetString();

        error.Should().Be($"{nameof(City)} with code : {duplicateCode} already exist");

        // Handler must NOT attempt to add the entity
        _cityRepoMock.Verify(r =>
            r.AddAsync(It.IsAny<City>(), It.IsAny<CancellationToken>()),
            Times.Never,
            "no insertion should happen when the code is already taken");
    }
}
