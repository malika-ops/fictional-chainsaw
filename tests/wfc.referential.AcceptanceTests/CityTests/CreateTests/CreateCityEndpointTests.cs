using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BuildingBlocks.Application.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using wfc.referential.Application.Cities.Dtos;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.CityAggregate;
using wfc.referential.Domain.RegionAggregate;
using Xunit;


namespace wfc.referential.AcceptanceTests.CityTests.CreateTests;

public class CreateCityEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<ICityRepository> _repoMock = new();

    public CreateCityEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        // clone the factory and customise the host
        var customisedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                // 🧹 Remove concrete registrations that hit the DB / Redis
                services.RemoveAll<ICityRepository>();
                services.RemoveAll<ICacheService>();

                // 🪄  Set up mock behaviour (echoes entity back, as if EF saved it)
                _repoMock
                    .Setup(r => r.AddCityAsync(It.IsAny<City>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((City r, CancellationToken _) => r);

                // 🔌 Plug mocks back in
                services.AddSingleton(_repoMock.Object);
                services.AddSingleton(cacheMock.Object);
            });
        });

        _client = customisedFactory.CreateClient();
    }

    [Fact(DisplayName = "POST /api/cities returns 200 and Guid (fixture version)")]
    public async Task Post_ShouldReturn200_AndId_WhenRequestIsValid()
    {
        // Arrange
        var payload = new CreateCityRequest
        {
            CityCode= "NYC",
            CityName= "New York City",
            Abbreviation= "NYC",
            RegionId= Guid.Parse("a3b8b953-2489-49c5-aac4-c97df06d5060"),
            TimeZone= "America/New_York",
            TaxZone= "TaxZone1"
        };

        // Act
        var response = await _client.PostAsJsonAsync<CreateCityRequest>("/api/cities", payload);
        var returnedId = await response.Content.ReadFromJsonAsync<Guid>();

        // Assert (FluentAssertions)
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returnedId.Should().NotBeEmpty();

        // verify repository interaction using *FluentAssertions on Moq invocations
        _repoMock.Verify(r =>
            r.AddCityAsync(It.Is<City>(r =>
                    r.Code == payload.CityCode &&
                    r.Name == payload.CityName),
                    It.IsAny<CancellationToken>()
            ),
            Times.Once
        );

    }

    [Fact(DisplayName = "POST /api/cities returns 400 & problem‑details when Code is missing")]
    public async Task Post_ShouldReturn400_WhenValidationFails()
    {
        var invalidPayload = new CreateCityRequest
        {
            CityName = "New York City",
            Abbreviation = "NYC",
            RegionId = Guid.Parse("a3b8b953-2489-49c5-aac4-c97df06d5060"),
            TimeZone = "America/New_York",
            TaxZone = "TaxZone1"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/cities", invalidPayload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var root = doc!.RootElement;
        root.GetProperty("title").GetString().Should().Be("Bad Request");
        root.GetProperty("status").GetInt32().Should().Be(400);

        root.GetProperty("errors")
            .GetProperty("cityCode")[0].GetString()
            .Should().Be("Code is required");

        // the handler must NOT be reached
        _repoMock.Verify(r =>
            r.AddCityAsync(It.IsAny<City>(), It.IsAny<CancellationToken>()),
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
            TimeZone = "America/New_York",
            TaxZone = "TaxZone1"
        };

        // ---------- Act ----------
        var response = await _client.PostAsJsonAsync("/api/cities", invalidPayload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // ---------- Assert ----------
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var root = doc!.RootElement;

        root.GetProperty("title").GetString().Should().Be("Bad Request");
        root.GetProperty("status").GetInt32().Should().Be(400);

        var errors = root.GetProperty("errors");

        errors.GetProperty("cityName")[0].GetString()
              .Should().Be("Name is required");

        errors.GetProperty("cityCode")[0].GetString()
              .Should().Be("Code is required");

        // handler must NOT run on validation failure
        _repoMock.Verify(r =>
            r.AddCityAsync(It.IsAny<City>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact(DisplayName = "POST /api/cities returns 400 when Code already exists")]
    public async Task Post_ShouldReturn400_WhenCodeAlreadyExists()
    {
        // Arrange 
        const string duplicateCode = "99";

        // Tell the repo mock that the code already exists
        var city = City.Create(CityId.Of(Guid.NewGuid()), "", "", "", "", RegionId.Of(Guid.NewGuid()), "");

        _repoMock
            .Setup(r => r.GetByCodeAsync(duplicateCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(city);

        var payload = new CreateCityRequest
        {
            CityCode = duplicateCode,
            CityName = "New York City",
            Abbreviation = "NYC",
            RegionId = Guid.Parse("a3b8b953-2489-49c5-aac4-c97df06d5060"),
            TimeZone = "America/New_York",
            TaxZone = "TaxZone1"
        }; 

        // Act
        var response = await _client.PostAsJsonAsync("/api/cities", payload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var root = doc!.RootElement;
        var error = root.GetProperty("errors").GetString();

        error.Should().Be($"{nameof(City)} with code : {duplicateCode} already exist");

        // Handler must NOT attempt to add the entity
        _repoMock.Verify(r =>
            r.AddCityAsync(It.IsAny<City>(), It.IsAny<CancellationToken>()),
            Times.Never,
            "no insertion should happen when the code is already taken");
    }
}
