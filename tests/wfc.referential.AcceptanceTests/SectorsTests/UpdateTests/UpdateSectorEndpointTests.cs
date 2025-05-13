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

namespace wfc.referential.AcceptanceTests.SectorsTests.UpdateTests;

public class UpdateSectorEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<ISectorRepository> _repoMock = new();
    private readonly Mock<ICityRepository> _cityRepoMock = new();

    public UpdateSectorEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var customisedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<ISectorRepository>();
                services.RemoveAll<ICityRepository>();
                services.RemoveAll<ICacheService>();

                // Default noop for Update
                _repoMock
                    .Setup(r => r.UpdateSectorAsync(It.IsAny<Sector>(),
                                                   It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

                // Set up country, city, region mocks to return valid entities
                var countryId = CountryId.Of(Guid.Parse("11111111-1111-1111-1111-111111111111"));
                var cityId = CityId.Of(Guid.Parse("22222222-2222-2222-2222-222222222222"));
                var regionId = RegionId.Of(Guid.Parse("33333333-3333-3333-3333-333333333333"));

                _cityRepoMock
                    .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(City.Create(cityId, "CITY1", "Test City", "GMT", "TZ", regionId, "TC"));

                services.AddSingleton(_repoMock.Object);
                services.AddSingleton(_cityRepoMock.Object);
                services.AddSingleton(cacheMock.Object);
            });
        });

        _client = customisedFactory.CreateClient();
    }

    // Helper to create a test sector
    private static Sector CreateTestSector(Guid id, string code, string name)
    {
        var countryId = CountryId.Of(Guid.Parse("11111111-1111-1111-1111-111111111111"));
        var cityId = CityId.Of(Guid.Parse("22222222-2222-2222-2222-222222222222"));
        var regionId = RegionId.Of(Guid.Parse("33333333-3333-3333-3333-333333333333"));

        var country = Country.Create(countryId, "TC", "Test Country", "TC", "TC", "TCO", "+0", "0", false, false, 2,
            true,
            new Domain.MonetaryZoneAggregate.MonetaryZoneId(Guid.NewGuid())
        );

        var city = City.Create(cityId, "CITY1", "Test City", "GMT", "TZ", regionId, "TC");

        var region = Region.Create(regionId, "REG1", "Test Region", countryId);

        return Sector.Create(new SectorId(id), code, name, city);
    }

    [Fact(DisplayName = "PUT /api/sectors/{id} returns 200 when update succeeds")]
    public async Task Put_ShouldReturn200_WhenUpdateIsSuccessful()
    {
        // Arrange
        var id = Guid.NewGuid();
        var oldSector = CreateTestSector(id, "OLD-SEC", "Old Sector");

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<SectorId>(sid => sid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(oldSector);

        _repoMock.Setup(r => r.GetByCodeAsync("NEW-SEC", It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Sector?)null);   // Code is unique

        Sector? updated = null;
        _repoMock.Setup(r => r.UpdateSectorAsync(It.IsAny<Sector>(),
                                                It.IsAny<CancellationToken>()))
                 .Callback<Sector, CancellationToken>((s, _) => updated = s)
                 .Returns(Task.CompletedTask);

        var countryId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var cityId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var regionId = Guid.Parse("33333333-3333-3333-3333-333333333333");

        var payload = new
        {
            SectorId = id,
            Code = "NEW-SEC",
            Name = "New Sector Name",
            CountryId = countryId,
            CityId = cityId,
            RegionId = regionId,
            IsEnabled = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/sectors/{id}", payload);
        var returned = await response.Content.ReadFromJsonAsync<Guid>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returned.Should().Be(id);

        updated!.Code.Should().Be("NEW-SEC");
        updated.Name.Should().Be("New Sector Name");
        updated.IsEnabled.Should().BeTrue();

        _repoMock.Verify(r => r.UpdateSectorAsync(It.IsAny<Sector>(),
                                                 It.IsAny<CancellationToken>()),
                          Times.Once);
    }

    [Fact(DisplayName = "PUT /api/sectors/{id} returns 200 when updating IsEnabled to false")]
    public async Task Put_ShouldReturn200_WhenUpdatingIsEnabledToFalse()
    {
        // Arrange
        var id = Guid.NewGuid();
        var oldSector = CreateTestSector(id, "SECTOR-ENABLED", "Enabled Sector");

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<SectorId>(sid => sid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(oldSector);

        Sector? updated = null;
        _repoMock.Setup(r => r.UpdateSectorAsync(It.IsAny<Sector>(),
                                                It.IsAny<CancellationToken>()))
                 .Callback<Sector, CancellationToken>((s, _) => updated = s)
                 .Returns(Task.CompletedTask);

        var cityId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        var payload = new
        {
            SectorId = id,
            Code = "SECTOR-ENABLED",
            Name = "Enabled Sector",
            CityId = cityId,
            IsEnabled = false
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/sectors/{id}", payload);
        var returned = await response.Content.ReadFromJsonAsync<Guid>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returned.Should().Be(id);

        updated!.IsEnabled.Should().BeFalse();

        _repoMock.Verify(r => r.UpdateSectorAsync(It.IsAny<Sector>(),
                                                 It.IsAny<CancellationToken>()),
                          Times.Once);
    }

    [Fact(DisplayName = "PUT /api/sectors/{id} returns 400 when Name is missing")]
    public async Task Put_ShouldReturn400_WhenNameMissing()
    {
        // Arrange
        var id = Guid.NewGuid();
        var payload = new
        {
            SectorId = id,
            Code = "SEC-001",
            // Name omitted
            CountryId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            CityId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            IsEnabled = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/sectors/{id}", payload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors")
            .GetProperty("name")[0].GetString()
            .Should().Be("Name is required");

        _repoMock.Verify(r => r.UpdateSectorAsync(It.IsAny<Sector>(),
                                                 It.IsAny<CancellationToken>()),
                         Times.Never);
    }

    [Fact(DisplayName = "PUT /api/sectors/{id} returns 400 when new code already exists")]
    public async Task Put_ShouldReturn400_WhenCodeAlreadyExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existingId = Guid.NewGuid();
        var existing = CreateTestSector(existingId, "DUPE-CODE", "Existing Sector");
        var target = CreateTestSector(id, "OLD-CODE", "Target Sector");

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<SectorId>(sid => sid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(target);

        _repoMock.Setup(r => r.GetByCodeAsync("DUPE-CODE", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(existing); // Duplicate code

        var payload = new
        {
            SectorId = id,
            Code = "DUPE-CODE",        // Duplicate
            Name = "Updated Sector",
            CountryId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            CityId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            IsEnabled = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/sectors/{id}", payload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors").GetString()
           .Should().Be("Sector with code DUPE-CODE already exists.");

        _repoMock.Verify(r => r.UpdateSectorAsync(It.IsAny<Sector>(),
                                                 It.IsAny<CancellationToken>()),
                         Times.Never);
    }

    [Fact(DisplayName = "PUT /api/sectors/{id} returns 404 when sector doesn't exist")]
    public async Task Put_ShouldReturn404_WhenSectorDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<SectorId>(sid => sid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Sector?)null);

        var payload = new
        {
            SectorId = id,
            Code = "NEW-CODE",
            Name = "New Name",
            CountryId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            CityId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            IsEnabled = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/sectors/{id}", payload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors").GetString()
           .Should().Be($"Sector with ID {id} not found");

        _repoMock.Verify(r => r.UpdateSectorAsync(It.IsAny<Sector>(),
                                                 It.IsAny<CancellationToken>()),
                         Times.Never);
    }
}