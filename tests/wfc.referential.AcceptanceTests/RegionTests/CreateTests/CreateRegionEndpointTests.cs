using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Moq;
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.CurrencyAggregate;
using wfc.referential.Domain.MonetaryZoneAggregate;
using wfc.referential.Domain.RegionAggregate;
using Xunit;


namespace wfc.referential.AcceptanceTests.RegionTests.CreateTests;

public class CreateRegionEndpointTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{

    [Fact(DisplayName = "POST /api/regions returns 200 and Guid (fixture version)")]
    public async Task Post_ShouldReturn200_AndId_WhenRequestIsValid()
    {
        // Arrange
        var payload = new
        {
            Code = "999",
            Name = "Casablanca-Settat",
            CountryId = Guid.Parse("50ed04f5-d16b-49c6-af46-b3ea7dfb8cb1")
        };

        _countryRepoMock.Setup(
            r => r.GetByIdAsync(It.IsAny<CountryId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Country.Create(
                CountryId.Of(payload.CountryId),
                "Morocco",
                "MA",
                "Moroccan Dirham", "iso2", "iso3", "dialingCode", "GMT", true, true, 2,
                MonetaryZoneId.Of(Guid.NewGuid()), CurrencyId.Of(Guid.NewGuid())));

        // Act
        var response = await _client.PostAsJsonAsync("/api/regions", payload);
        var returnedId = await response.Content.ReadFromJsonAsync<Guid>();

        // Assert (FluentAssertions)
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        returnedId.Should().NotBeEmpty();

        // verify repository interaction using *FluentAssertions on Moq invocations
        _regionRepoMock.Verify(r =>
            r.AddAsync(It.Is<Region>(r =>
                    r.Code == payload.Code &&
                    r.Name == payload.Name &&
                    r.CountryId == CountryId.Of(payload.CountryId)),
                    It.IsAny<CancellationToken>()
            ),
            Times.Once
        );

    }

    [Fact(DisplayName = "POST /api/regions returns 400 & problem‑details when Code is missing")]
    public async Task Post_ShouldReturn400_WhenValidationFails()
    {
        // Arrange
        var invalidPayload = new
        {
            // Code intentionally omitted to trigger validation error
            Name = "United States Dollar",
            CountryId = Guid.Parse("50ed04f5-d16b-49c6-af46-b3ea7dfb8cb1")
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/regions", invalidPayload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var root = doc!.RootElement;
        root.GetProperty("title").GetString().Should().Be("One or more validation errors occurred.");
        root.GetProperty("status").GetInt32().Should().Be(400);

        root.GetProperty("errors")
            .GetProperty("Code")[0].GetString()
            .Should().Be("Code is required");

        // the handler must NOT be reached
        _regionRepoMock.Verify(r =>
            r.AddAsync(It.IsAny<Region>(), It.IsAny<CancellationToken>()),
            Times.Never,
            "when validation fails, the command handler should not be executed");
    }

    [Fact(DisplayName = "POST /api/regions returns 400 & problem‑details when Name and Code are missing")]
    public async Task Post_ShouldReturn400_WhenNameAndCodeAreMissing()
    {
        // ---------- Arrange ----------
        var invalidPayload = new
        {
            CountryId = Guid.Parse("50ed04f5-d16b-49c6-af46-b3ea7dfb8cb1")
        };

        // ---------- Act ----------
        var response = await _client.PostAsJsonAsync("/api/regions", invalidPayload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // ---------- Assert ----------
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var root = doc!.RootElement;

        root.GetProperty("title").GetString().Should().Be("One or more validation errors occurred.");
        root.GetProperty("status").GetInt32().Should().Be(400);

        var errors = root.GetProperty("errors");

        errors.GetProperty("Name")[0].GetString()
              .Should().Be("Name is required");

        errors.GetProperty("Code")[0].GetString()
              .Should().Be("Code is required");

        // handler must NOT run on validation failure
        _regionRepoMock.Verify(r =>
            r.AddAsync(It.IsAny<Region>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact(DisplayName = "POST /api/regions returns 409 Conflict when Code already exists")]
    public async Task Post_ShouldReturn409Conflict_WhenCodeAlreadyExists()
    {
        // Arrange 
        const string duplicateCode = "99";

        // Tell the repo mock that the code already exists
        var region = Region.Create(
            RegionId.Of(Guid.NewGuid()),
            duplicateCode,
            "Swiss Franc",
            CountryId.Of(Guid.NewGuid()));

        _regionRepoMock
            .Setup(r => r.GetOneByConditionAsync(It.IsAny<Expression<Func<Region, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(region);

        var payload = new
        {
            Code = duplicateCode,
            Name = "Swiss Franc",
            CountryId = Guid.NewGuid()
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/regions", payload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var root = doc!.RootElement;
        var error = root.GetProperty("errors").GetProperty("message").GetString();

        error.Should().Be($"Region with code : {duplicateCode} already exist");

        // Handler must NOT attempt to add the entity
        _regionRepoMock.Verify(r =>
            r.AddAsync(It.IsAny<Region>(), It.IsAny<CancellationToken>()),
            Times.Never,
            "no insertion should happen when the code is already taken");
    }
}
