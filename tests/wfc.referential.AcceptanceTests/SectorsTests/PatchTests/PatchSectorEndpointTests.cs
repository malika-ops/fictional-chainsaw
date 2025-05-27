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
using wfc.referential.Domain.CurrencyAggregate;
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

                // Default setup for Update
                _repoMock.Setup(r => r.Update(It.IsAny<Sector>()));
                _repoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

                // FIXED: Set up city mocks to return valid entities with correct parameter type
                _cityRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<CityId>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((CityId cityId, CancellationToken _) =>
                        City.Create(cityId, "TEST", "Test City", "GMT",
                                   new RegionId(Guid.NewGuid()), "TC"));

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
            new Domain.MonetaryZoneAggregate.MonetaryZoneId(Guid.NewGuid()),
            new CurrencyId(Guid.NewGuid())
            );

        var city = City.Create(cityId, "CITY1", "Test City", "GMT", regionId, "TC");

        return Sector.Create(new SectorId(id), code, name, cityId);
    }

    [Fact(DisplayName = "PATCH /api/sectors/{id} returns 200 and patches only the provided fields")]
    public async Task Patch_ShouldReturn200_AndPatchOnlyProvidedFields()
    {
        // Arrange
        var id = Guid.NewGuid();
        var sector = CreateTestSector(id, "OLD-CODE", "Old Name");

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<SectorId>(sid => sid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(sector);

        _repoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Sector, bool>>>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Sector?)null);   // Code is unique

        Sector? updated = null;
        _repoMock.Setup(r => r.Update(It.IsAny<Sector>()))
                 .Callback<Sector>(s => updated = s);

        var payload = new
        {
            SectorId = id,
            Code = "NEW-CODE"
            // Name intentionally omitted - should not change
        };

        // Act
        var response = await _client.PatchAsync($"/api/sectors/{id}", JsonContent.Create(payload));
        var returned = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returned.Should().BeTrue();

        updated!.Code.Should().Be("NEW-CODE");
        updated.Name.Should().Be("Old Name");  // Name should not change

        _repoMock.Verify(r => r.Update(It.IsAny<Sector>()), Times.Once);
        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
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
        _repoMock.Setup(r => r.Update(It.IsAny<Sector>()))
                 .Callback<Sector>(s => updated = s);

        var payload = new
        {
            SectorId = id,
            IsEnabled = false
        };

        // Act
        var response = await _client.PatchAsync($"/api/sectors/{id}", JsonContent.Create(payload));
        var returned = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returned.Should().BeTrue();

        updated!.IsEnabled.Should().BeFalse();
        updated.Code.Should().Be("CODE-ENABLE");  // Code should not change
        updated.Name.Should().Be("Sector Name");  // Name should not change

        _repoMock.Verify(r => r.Update(It.IsAny<Sector>()), Times.Once);
        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
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

        _repoMock.Verify(r => r.Update(It.IsAny<Sector>()), Times.Never);
    }

    [Fact(DisplayName = "PATCH /api/sectors/{id} returns 409 when new code already exists")]
    public async Task Patch_ShouldReturn409_WhenNewCodeAlreadyExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existingId = Guid.NewGuid();

        var existing = CreateTestSector(existingId, "DUPE-CODE", "Existing Sector");
        var target = CreateTestSector(id, "OLD-CODE", "Target Sector");

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<SectorId>(sid => sid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(target);

        _repoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Sector, bool>>>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(existing); // Duplicate code found via condition

        var payload = new
        {
            SectorId = id,
            Code = "DUPE-CODE"  // This code already exists for another sector
        };

        // Act
        var response = await _client.PatchAsync($"/api/sectors/{id}", JsonContent.Create(payload));
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        doc!.RootElement.GetProperty("errors").GetString()
           .Should().Be("Sector with code DUPE-CODE already exists.");

        _repoMock.Verify(r => r.Update(It.IsAny<Sector>()), Times.Never);
    }

    [Fact(DisplayName = "PATCH /api/sectors/{id} returns 400 when provided City doesn't exist")]
    public async Task Patch_ShouldReturn400_WhenProvidedCityDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();
        var nonExistentCityId = Guid.Parse("99999979-9999-9999-9999-999999999999");

        var sector = CreateTestSector(id, "SEC-001", "Test Sector");

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<SectorId>(sid => sid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(sector);

        // Setup city repository to return null for this ID
        _cityRepoMock
            .Setup(r => r.GetByIdAsync(CityId.Of(nonExistentCityId), It.IsAny<CancellationToken>()))
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

        _repoMock.Verify(r => r.Update(It.IsAny<Sector>()), Times.Never);
    }

    [Fact(DisplayName = "PATCH /api/sectors/{id} patches only Name field")]
    public async Task Patch_ShouldUpdateOnlyNameField_WhenOnlyNameProvided()
    {
        // Arrange
        var id = Guid.NewGuid();
        var sector = CreateTestSector(id, "SEC-001", "Old Name");

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<SectorId>(sid => sid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(sector);

        Sector? updated = null;
        _repoMock.Setup(r => r.Update(It.IsAny<Sector>()))
                 .Callback<Sector>(s => updated = s);

        var payload = new
        {
            SectorId = id,
            Name = "Updated Name Only"
        };

        // Act
        var response = await _client.PatchAsync($"/api/sectors/{id}", JsonContent.Create(payload));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        updated!.Name.Should().Be("Updated Name Only");
        updated.Code.Should().Be("SEC-001"); // Should remain unchanged
    }

    [Fact(DisplayName = "PATCH /api/sectors/{id} patches only CityId field")]
    public async Task Patch_ShouldUpdateOnlyCityField_WhenOnlyCityProvided()
    {
        // Arrange
        var id = Guid.NewGuid();
        var newCityId = Guid.NewGuid();
        var sector = CreateTestSector(id, "SEC-001", "Test Sector");

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<SectorId>(sid => sid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(sector);

        Sector? updated = null;
        _repoMock.Setup(r => r.Update(It.IsAny<Sector>()))
                 .Callback<Sector>(s => updated = s);

        var payload = new
        {
            SectorId = id,
            CityId = newCityId
        };

        // Act
        var response = await _client.PatchAsync($"/api/sectors/{id}", JsonContent.Create(payload));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        updated!.CityId.Value.Should().Be(newCityId);
        updated.Code.Should().Be("SEC-001"); // Should remain unchanged
        updated.Name.Should().Be("Test Sector"); // Should remain unchanged
    }

    [Fact(DisplayName = "PATCH /api/sectors/{id} handles multiple field updates")]
    public async Task Patch_ShouldUpdateMultipleFields_WhenMultipleFieldsProvided()
    {
        // Arrange
        var id = Guid.NewGuid();
        var newCityId = Guid.NewGuid();
        var sector = CreateTestSector(id, "OLD-CODE", "Old Name");

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<SectorId>(sid => sid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(sector);

        _repoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Sector, bool>>>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Sector?)null);

        Sector? updated = null;
        _repoMock.Setup(r => r.Update(It.IsAny<Sector>()))
                 .Callback<Sector>(s => updated = s);

        var payload = new
        {
            SectorId = id,
            Code = "NEW-CODE",
            Name = "New Name",
            CityId = newCityId,
            IsEnabled = false
        };

        // Act
        var response = await _client.PatchAsync($"/api/sectors/{id}", JsonContent.Create(payload));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        updated!.Code.Should().Be("NEW-CODE");
        updated.Name.Should().Be("New Name");
        updated.CityId.Value.Should().Be(newCityId);
        updated.IsEnabled.Should().BeFalse();
    }

    [Fact(DisplayName = "PATCH /api/sectors/{id} allows patching with same code for same sector")]
    public async Task Patch_ShouldAllowSameCodeForSameSector()
    {
        // Arrange
        var id = Guid.NewGuid();
        var sector = CreateTestSector(id, "SAME-CODE", "Old Name");

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<SectorId>(sid => sid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(sector);

        _repoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Sector, bool>>>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(sector); // Same sector with same code - this should be allowed

        Sector? updated = null;
        _repoMock.Setup(r => r.Update(It.IsAny<Sector>()))
                 .Callback<Sector>(s => updated = s);

        var payload = new
        {
            SectorId = id,
            Code = "SAME-CODE", // Same code as existing
            Name = "New Name"
        };

        // Act
        var response = await _client.PatchAsync($"/api/sectors/{id}", JsonContent.Create(payload));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        updated!.Code.Should().Be("SAME-CODE");
        updated.Name.Should().Be("New Name");
    }

    [Fact(DisplayName = "PATCH /api/sectors/{id} validates route parameter matches body parameter")]
    public async Task Patch_ShouldReturnBadRequest_WhenRouteIdDifferentFromBodyId()
    {
        // Arrange
        var routeId = Guid.NewGuid();
        var bodyId = Guid.NewGuid(); // Different ID

        var payload = new
        {
            SectorId = bodyId, // Different from route
            Name = "New Name"
        };

        // Act
        var response = await _client.PatchAsync($"/api/sectors/{routeId}", JsonContent.Create(payload));

        // Assert
        // Note: This test will fail unless you add endpoint-level validation
        // The current handler doesn't validate route vs body ID mismatch
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        _repoMock.Verify(r => r.Update(It.IsAny<Sector>()), Times.Never);
    }
}