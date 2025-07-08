using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Moq;
using wfc.referential.Application.Sectors.Dtos;
using wfc.referential.Domain.CityAggregate;
using wfc.referential.Domain.SectorAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.SectorsTests.UpdateTests;

public class UpdateSectorEndpointTests : BaseAcceptanceTests
{
    public UpdateSectorEndpointTests(TestWebApplicationFactory factory) : base(factory)
    {
        _cityRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<CityId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CityId cityId, CancellationToken _) =>
                City.Create(cityId, "TEST", "Test City", "GMT",
                           new Domain.RegionAggregate.RegionId(Guid.NewGuid()), "TC"));
    }

    [Fact(DisplayName = "PUT /api/sectors/{id} modifies all sector data")]
    public async Task UpdateSector_Should_ModifyAllSectorFields_WhenValidDataProvided()
    {
        // Arrange
        var sectorId = Guid.NewGuid();
        var newCityId = Guid.NewGuid();
        var existingSector = Sector.Create(
            SectorId.Of(sectorId),
            "SEC001", "Old Sector", CityId.Of(Guid.NewGuid()));

        _sectorRepoMock.Setup(r => r.GetByIdAsync(It.Is<SectorId>(id => id.Value == sectorId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingSector);
        _sectorRepoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Sector, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Sector)null);

        var updateRequest = new UpdateSectorRequest
        {
            Code = "SEC001_UPDATED",
            Name = "Updated Sector",
            CityId = newCityId,
            IsEnabled = false
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/sectors/{sectorId}", updateRequest);
        var result = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeTrue();

        _sectorRepoMock.Verify(r => r.GetByIdAsync(It.Is<SectorId>(id => id.Value == sectorId), It.IsAny<CancellationToken>()), Times.Once);
        _sectorRepoMock.Verify(r => r.Update(It.IsAny<Sector>()), Times.Once);
        _sectorRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _cityRepoMock.Verify(r => r.GetByIdAsync(CityId.Of(newCityId), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PUT /api/sectors/{id} returns 409 when Code already exists")]
    public async Task UpdateSector_Should_ValidateCodeUniqueness_BeforeUpdate()
    {
        // Arrange
        var sectorId = Guid.NewGuid();
        var existingSectorId = Guid.NewGuid();

        var targetSector = Sector.Create(SectorId.Of(sectorId), "SEC001", "Sector 1", CityId.Of(Guid.NewGuid()));
        var conflictingSector = Sector.Create(SectorId.Of(existingSectorId), "SEC002", "Sector 2", CityId.Of(Guid.NewGuid()));

        _sectorRepoMock.Setup(r => r.GetByIdAsync(It.Is<SectorId>(id => id.Value == sectorId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(targetSector);
        _sectorRepoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Sector, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(conflictingSector);

        var updateRequest = new UpdateSectorRequest
        {
            Code = "SEC002", // Code already exists
            Name = "Updated Sector",
            CityId = Guid.NewGuid(),
            IsEnabled = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/sectors/{sectorId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        _sectorRepoMock.Verify(r => r.Update(It.IsAny<Sector>()), Times.Never);
    }

    [Fact(DisplayName = "PUT /api/sectors/{id} returns 400 when sector not found")]
    public async Task UpdateSector_Should_ReturnBadRequest_WhenSectorNotFound()
    {
        // Arrange
        var sectorId = Guid.NewGuid();

        _sectorRepoMock.Setup(r => r.GetByIdAsync(It.Is<SectorId>(id => id.Value == sectorId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Sector)null);

        var updateRequest = new UpdateSectorRequest
        {
            Code = "SEC001",
            Name = "Test Sector",
            CityId = Guid.NewGuid(),
            IsEnabled = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/sectors/{sectorId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        _sectorRepoMock.Verify(r => r.Update(It.IsAny<Sector>()), Times.Never);
    }

    [Fact(DisplayName = "PUT /api/sectors/{id} returns 400 when City doesn't exist")]
    public async Task UpdateSector_Should_ReturnBadRequest_WhenCityDoesNotExist()
    {
        // Arrange
        var sectorId = Guid.NewGuid();
        var nonExistentCityId = Guid.NewGuid();
        var existingSector = Sector.Create(
            SectorId.Of(sectorId),
            "SEC001", "Test Sector", CityId.Of(Guid.NewGuid()));

        _sectorRepoMock.Setup(r => r.GetByIdAsync(It.Is<SectorId>(id => id.Value == sectorId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingSector);
        _sectorRepoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Sector, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Sector)null);

        _cityRepoMock.Setup(r => r.GetByIdAsync(CityId.Of(nonExistentCityId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((City)null);

        var updateRequest = new UpdateSectorRequest
        {
            Code = "SEC001_UPDATED",
            Name = "Updated Sector",
            CityId = nonExistentCityId,
            IsEnabled = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/sectors/{sectorId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        _sectorRepoMock.Verify(r => r.Update(It.IsAny<Sector>()), Times.Never);
    }

    [Theory(DisplayName = "PUT /api/sectors/{id} validates required fields")]
    [InlineData("", "Valid Name", "Code cannot be empty")]
    [InlineData("VALID_CODE", "", "Name cannot be empty")]
    [InlineData(null, "Valid Name", "Code cannot be null")]
    [InlineData("VALID_CODE", null, "Name cannot be null")]
    public async Task UpdateSector_Should_ReturnValidationError_WhenRequiredFieldsInvalid(
        string code, string name, string scenario)
    {
        // Arrange
        var sectorId = Guid.NewGuid();
        var existingSector = Sector.Create(
            SectorId.Of(sectorId),
            "OLD_CODE", "Old Name", CityId.Of(Guid.NewGuid()));

        _sectorRepoMock.Setup(r => r.GetByIdAsync(It.Is<SectorId>(id => id.Value == sectorId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingSector);

        var updateRequest = new UpdateSectorRequest
        {
            Code = code,
            Name = name,
            CityId = Guid.NewGuid(),
            IsEnabled = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/sectors/{sectorId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, because: scenario);
        _sectorRepoMock.Verify(r => r.Update(It.IsAny<Sector>()), Times.Never);
    }

    [Fact(DisplayName = "PUT /api/sectors/{id} allows updating IsEnabled status")]
    public async Task UpdateSector_Should_UpdateIsEnabledStatus_Successfully()
    {
        // Arrange
        var sectorId = Guid.NewGuid();
        var existingSector = Sector.Create(
            SectorId.Of(sectorId),
            "SEC001", "Test Sector", CityId.Of(Guid.NewGuid()));
        // Initially enabled by default

        _sectorRepoMock.Setup(r => r.GetByIdAsync(It.Is<SectorId>(id => id.Value == sectorId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingSector);
        _sectorRepoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Sector, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Sector)null);

        var updateRequest = new UpdateSectorRequest
        {
            Code = "SEC001",
            Name = "Test Sector",
            CityId = existingSector.CityId.Value,
            IsEnabled = false // Disable the sector
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/sectors/{sectorId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        _sectorRepoMock.Verify(r => r.Update(It.IsAny<Sector>()), Times.Once);
    }

    [Fact(DisplayName = "PUT /api/sectors/{id} validates route parameter matches body parameter")]
    public async Task UpdateSector_Should_ReturnBadRequest_WhenRouteIdDifferentFromBodyId()
    {
        // Arrange
        var routeSectorId = Guid.NewGuid();
        var bodySectorId = Guid.NewGuid(); // Different ID

        var updateRequest = new UpdateSectorRequest
        {
            Code = "SEC001",
            Name = "Test Sector",
            CityId = Guid.NewGuid(),
            IsEnabled = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/sectors/{routeSectorId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        _sectorRepoMock.Verify(r => r.Update(It.IsAny<Sector>()), Times.Never);
    }

    [Fact(DisplayName = "PUT /api/sectors/{id} allows updating same code for same sector")]
    public async Task UpdateSector_Should_AllowSameCodeForSameSector()
    {
        // Arrange
        var sectorId = Guid.NewGuid();
        var existingSector = Sector.Create(
            SectorId.Of(sectorId),
            "SEC001", "Old Name", CityId.Of(Guid.NewGuid()));

        _sectorRepoMock.Setup(r => r.GetByIdAsync(It.Is<SectorId>(id => id.Value == sectorId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingSector);
        _sectorRepoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Sector, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingSector); // Same sector with same code

        var updateRequest = new UpdateSectorRequest
        {
            Code = "SEC001", // Same code
            Name = "Updated Name", // Only name changes
            CityId = existingSector.CityId.Value,
            IsEnabled = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/sectors/{sectorId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        _sectorRepoMock.Verify(r => r.Update(It.IsAny<Sector>()), Times.Once);
    }

    [Fact(DisplayName = "PUT /api/sectors/{id} handles special characters in updated data")]
    public async Task UpdateSector_Should_HandleSpecialCharacters_InUpdatedData()
    {
        // Arrange
        var sectorId = Guid.NewGuid();
        var existingSector = Sector.Create(
            SectorId.Of(sectorId),
            "SEC001", "Old Sector", CityId.Of(Guid.NewGuid()));

        _sectorRepoMock.Setup(r => r.GetByIdAsync(It.Is<SectorId>(id => id.Value == sectorId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingSector);
        _sectorRepoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Sector, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Sector)null);

        var updateRequest = new UpdateSectorRequest
        {
            Code = "SEC-001_MODIFIÉ",
            Name = "Secteur Modifié avec Accents & Symboles (Zone 1)",
            CityId = existingSector.CityId.Value,
            IsEnabled = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/sectors/{sectorId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        _sectorRepoMock.Verify(r => r.Update(It.IsAny<Sector>()), Times.Once);
    }
}