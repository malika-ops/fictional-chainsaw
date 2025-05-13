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
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.CityAggregate;
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.RegionAggregate;
using wfc.referential.Domain.SectorAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.SectorsTests.CreateTests;

public class CreateSectorEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<ISectorRepository> _repoMock = new();
    private readonly Mock<ICityRepository> _cityRepoMock = new();
    public CreateSectorEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        // Clone the factory and customize the host
        var customizedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                // Remove concrete registrations that hit the DB / Redis
                services.RemoveAll<ISectorRepository>();
                services.RemoveAll<ICityRepository>();
                services.RemoveAll<ICacheService>();

                // Set up mock behavior (echoes entity back, as if EF saved it)
                _repoMock
                    .Setup(r => r.AddSectorAsync(It.IsAny<Sector>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((Sector s, CancellationToken _) => s);

                // Set up country, city, region mocks to return valid entities
                var countryId = CountryId.Of(Guid.Parse("11111111-1111-1111-1111-111111111111"));
                var cityId = CityId.Of(Guid.Parse("22222222-2222-2222-2222-222222222222"));
                var regionId = RegionId.Of(Guid.Parse("33333333-3333-3333-3333-333333333333"));

                _cityRepoMock
                    .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(City.Create(cityId, "CITY1", "Test City", "GMT", "TZ", regionId, "TC"));


                // Plug mocks back in
                services.AddSingleton(_repoMock.Object);
                services.AddSingleton(_cityRepoMock.Object);
                services.AddSingleton(cacheMock.Object);
            });
        });

        _client = customizedFactory.CreateClient();
    }

    [Fact(DisplayName = "POST /api/sectors returns 200 and Guid when request is valid")]
    public async Task Post_ShouldReturn200_AndId_WhenRequestIsValid()
    {
        // Arrange
        var countryId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var cityId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var regionId = Guid.Parse("33333333-3333-3333-3333-333333333333");

        var payload = new
        {
            Code = "SECTOR1",
            Name = "Test Sector",
            CountryId = countryId,
            CityId = cityId,
            RegionId = regionId        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/sectors", payload);
        var returnedId = await response.Content.ReadFromJsonAsync<Guid>();

        // Assert (FluentAssertions)
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returnedId.Should().NotBeEmpty();

        // Verify repository interaction
        _repoMock.Verify(r =>
            r.AddSectorAsync(It.Is<Sector>(s =>
                    s.Code == payload.Code &&
                    s.Name == payload.Name &&
                    s.IsEnabled == true),
                    It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }

    [Fact(DisplayName = "POST /api/sectors returns 400 & problem-details when Code is missing")]
    public async Task Post_ShouldReturn400_WhenCodeIsMissing()
    {
        // Arrange
        var countryId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var cityId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        var invalidPayload = new
        {
            // Code intentionally omitted to trigger validation error
            Name = "Test Sector",
            CountryId = countryId,
            CityId = cityId,
            IsEnabled = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/sectors", invalidPayload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var root = doc!.RootElement;
        root.GetProperty("title").GetString().Should().Be("Bad Request");
        root.GetProperty("status").GetInt32().Should().Be(400);

        root.GetProperty("errors")
            .GetProperty("code")[0].GetString()
            .Should().Be("Code is required");

        // The handler must NOT be reached
        _repoMock.Verify(r =>
            r.AddSectorAsync(It.IsAny<Sector>(), It.IsAny<CancellationToken>()),
            Times.Never,
            "when validation fails, the command handler should not be executed");
    }

    [Fact(DisplayName = "POST /api/sectors returns 400 when Code already exists")]
    public async Task Post_ShouldReturn400_WhenCodeAlreadyExists()
    {
        // Arrange 
        const string duplicateCode = "SECTOR2";
        var countryId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var cityId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        // Tell the repo mock that the code already exists
        var existingSector = Sector.Create(
            SectorId.Of(Guid.NewGuid()),
            duplicateCode,
            "Existing Sector",
            City.Create(CityId.Of(cityId), "CITY1", "Test City", "GMT", "TZ", RegionId.Of(Guid.NewGuid()), "TC")
        );

        _repoMock
            .Setup(r => r.GetByCodeAsync(duplicateCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingSector);

        var payload = new
        {
            Code = duplicateCode,
            Name = "New Sector",
            CountryId = countryId,
            CityId = cityId,
            IsEnabled = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/sectors", payload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var root = doc!.RootElement;
        var error = root.GetProperty("errors").GetString();

        error.Should().Be($"Sector with code {duplicateCode} already exists.");

        // Handler must NOT attempt to add the entity
        _repoMock.Verify(r =>
            r.AddSectorAsync(It.IsAny<Sector>(), It.IsAny<CancellationToken>()),
            Times.Never,
            "no insertion should happen when the code is already taken");
    }

    [Fact(DisplayName = "POST /api/sectors returns 400 when City is not found")]
    public async Task Post_ShouldReturn400_WhenCountryNotFound()
    {
        // Arrange
        var nonExistentCityId = Guid.Parse("99999999-9999-9999-9999-999999999999");

        // Setup country repository to return null for this ID
        _cityRepoMock
            .Setup(r => r.GetByIdAsync(nonExistentCityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((City?)null);

        var payload = new
        {
            Code = "SECTOR3",
            Name = "Test Sector",
            CityId = nonExistentCityId,
            IsEnabled = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/sectors", payload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var root = doc!.RootElement;
        var error = root.GetProperty("errors").GetString();

        error.Should().Be($"City with ID {nonExistentCityId} not found");

        // Handler must NOT attempt to add the entity
        _repoMock.Verify(r =>
            r.AddSectorAsync(It.IsAny<Sector>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}