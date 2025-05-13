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

namespace wfc.referential.AcceptanceTests.SectorsTests.PatchTests;

public class PatchSectorEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<ISectorRepository> _repoMock = new();
    private readonly Mock<ICityRepository> _cityRepoMock = new();

    public PatchSectorEndpointTests(WebApplicationFactory<Program> factory)
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
            new Domain.MonetaryZoneAggregate.MonetaryZoneId(Guid.NewGuid()));

        var city = City.Create(cityId, "CITY1", "Test City", "GMT", "TZ", regionId, "TC");

        var region = Region.Create(regionId, "REG1", "Test Region", countryId);

        return Sector.Create(new SectorId(id), code, name, city);
    }

    [Fact(DisplayName = "PATCH /api/sectors/{id} returns 200 and patches only the provided fields")]
    public async Task Patch_ShouldReturn200_AndPatchOnlyProvidedFields()
    {
        // Arrange
        var id = Guid.NewGuid();
        var sector = CreateTestSector(id, "OLD-CODE", "Old Name");

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<SectorId>(sid => sid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(sector);

        _repoMock.Setup(r => r.GetByCodeAsync("NEW-CODE", It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Sector?)null);   // Code is unique

        Sector? updated = null;
        _repoMock.Setup(r => r.UpdateSectorAsync(It.IsAny<Sector>(),
                                                It.IsAny<CancellationToken>()))
                 .Callback<Sector, CancellationToken>((s, _) => updated = s)
                 .Returns(Task.CompletedTask);

        var payload = new
        {
            SectorId = id,
            Code = "NEW-CODE"
            // Name intentionally omitted - should not change
        };

        // Act
        var response = await _client.PatchAsync($"/api/sectors/{id}", JsonContent.Create(payload));
        var returned = await response.Content.ReadFromJsonAsync<Guid>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returned.Should().Be(id);

        updated!.Code.Should().Be("NEW-CODE");
        updated.Name.Should().Be("Old Name");  // Name should not change

        _repoMock.Verify(r => r.UpdateSectorAsync(It.IsAny<Sector>(),
                                                 It.IsAny<CancellationToken>()),
                          Times.Once);
    }

    [Fact(DisplayName = "PATCH /api/sectors/{id} returns 200 and updates IsEnabled")]
    public async Task Patch_ShouldReturn200_AndUpdateIsEnabled()
    {
        // Arrange
        var id = Guid.NewGuid();
        var sector = CreateTestSector(id, "CODE-ENABLE", "Sector Name");

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<SectorId>(sid => sid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(sector);

        Sector? updated = null;
        _repoMock.Setup(r => r.UpdateSectorAsync(It.IsAny<Sector>(),
                                                It.IsAny<CancellationToken>()))
                 .Callback<Sector, CancellationToken>((s, _) => updated = s)
                 .Returns(Task.CompletedTask);

        var payload = new
        {
            SectorId = id,
            IsEnabled = false
        };

        // Act
        var response = await _client.PatchAsync($"/api/sectors/{id}", JsonContent.Create(payload));
        var returned = await response.Content.ReadFromJsonAsync<Guid>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returned.Should().Be(id);

        updated!.IsEnabled.Should().BeFalse();
        updated.Code.Should().Be("CODE-ENABLE");  // Code should not change
        updated.Name.Should().Be("Sector Name");  // Name should not change

        _repoMock.Verify(r => r.UpdateSectorAsync(It.IsAny<Sector>(),
                                                 It.IsAny<CancellationToken>()),
                          Times.Once);
    }

    [Fact(DisplayName = "PATCH /api/sectors/{id} returns 400 when sector doesn't exist")]
    public async Task Patch_ShouldReturn400_WhenSectorDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<SectorId>(sid => sid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Sector?)null);

        var payload = new
        {
            SectorId = id,
            Name = "New Name"
        };

        // Act
        var response = await _client.PatchAsync($"/api/sectors/{id}", JsonContent.Create(payload));
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors").GetString()
           .Should().Be("Sector not found");

        _repoMock.Verify(r => r.UpdateSectorAsync(It.IsAny<Sector>(),
                                                 It.IsAny<CancellationToken>()),
                         Times.Never);
    }

    [Fact(DisplayName = "PATCH /api/sectors/{id} returns 400 when new code already exists")]
    public async Task Patch_ShouldReturn400_WhenNewCodeAlreadyExists()
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
            Code = "DUPE-CODE"  // This code already exists for another sector
        };

        // Act
        var response = await _client.PatchAsync($"/api/sectors/{id}", JsonContent.Create(payload));
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors").GetString()
           .Should().Be("Sector with code DUPE-CODE already exists.");

        _repoMock.Verify(r => r.UpdateSectorAsync(It.IsAny<Sector>(),
                                                 It.IsAny<CancellationToken>()),
                         Times.Never);
    }

    [Fact(DisplayName = "PATCH /api/sectors/{id} returns 400 when provided City doesn't exist")]
    public async Task Patch_ShouldReturn400_WhenProvidedCountryDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();
        var nonExistentCityId = Guid.Parse("99999979-9999-9999-9999-999999999999");

        var sector = CreateTestSector(id, "SEC-001", "Test Sector");

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<SectorId>(sid => sid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(sector);

        // Setup country repository to return null for this ID
        _cityRepoMock
            .Setup(r => r.GetByIdAsync(nonExistentCityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((City?)null);

        var payload = new
        {
            SectorId = id,
            CityId = nonExistentCityId
        };

        // Act
        var response = await _client.PatchAsync($"/api/sectors/{id}", JsonContent.Create(payload));
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors").GetString()
           .Should().Be($"City with ID {nonExistentCityId} not found");

        _repoMock.Verify(r => r.UpdateSectorAsync(It.IsAny<Sector>(),
                                                 It.IsAny<CancellationToken>()),
                         Times.Never);
    }
}