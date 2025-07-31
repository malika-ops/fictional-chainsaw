using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Moq;
using wfc.referential.Application.Sectors.Dtos;
using wfc.referential.Domain.CityAggregate;
using wfc.referential.Domain.SectorAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.SectorsTests.CreateTests;

public class CreateSectorEndpointTests : BaseAcceptanceTests
{
    public CreateSectorEndpointTests(TestWebApplicationFactory factory) : base(factory)
    {

        _sectorRepoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Sector, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Sector)null);

        // FIXED: Setup city repository with correct parameter type
        _cityRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<CityId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CityId cityId, CancellationToken _) =>
                City.Create(cityId, "TEST", "Test City", "GMT",
                           new Domain.RegionAggregate.RegionId(Guid.NewGuid()), "TC"));

    }

    [Fact(DisplayName = "POST /api/sectors creates sector with all required fields")]
    public async Task CreateSector_Should_CreateNewSector_WhenAllRequiredFieldsProvided()
    {
        // Arrange
        var cityId = Guid.NewGuid();
        var createRequest = new CreateSectorRequest
        {
            Code = "SEC001",
            Name = "Downtown Sector",
            CityId = cityId
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/sectors", createRequest);
        var sectorId = await response.Content.ReadFromJsonAsync<Guid>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        sectorId.Should().NotBeEmpty();

        _sectorRepoMock.Verify(r => r.AddAsync(It.Is<Sector>(s =>
            s.Code == "SEC001" &&
            s.Name == "Downtown Sector" &&
            s.CityId.Value == cityId &&
            s.IsEnabled == true), It.IsAny<CancellationToken>()), Times.Once);

        _sectorRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "POST /api/sectors returns 409 when duplicate Code is provided")]
    public async Task CreateSector_Should_ReturnConflictError_WhenSectorCodeAlreadyExists()
    {
        // Arrange
        var existingSector = Sector.Create(
            SectorId.Of(Guid.NewGuid()),
            "SEC001", "Existing Sector", CityId.Of(Guid.NewGuid()));

        _sectorRepoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Sector, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingSector);

        var duplicateRequest = new CreateSectorRequest
        {
            Code = "SEC001",
            Name = "Another Sector",
            CityId = Guid.NewGuid()
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/sectors", duplicateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        _sectorRepoMock.Verify(r => r.AddAsync(It.IsAny<Sector>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "POST /api/sectors auto-generates sector ID")]
    public async Task CreateSector_Should_AutoGenerateSectorId_WhenSectorIsCreated()
    {
        // Arrange
        var createRequest = new CreateSectorRequest
        {
            Code = "SEC002",
            Name = "North Sector",
            CityId = Guid.NewGuid()
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/sectors", createRequest);
        var sectorId = await response.Content.ReadFromJsonAsync<Guid>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        sectorId.Should().NotBeEmpty();

        _sectorRepoMock.Verify(r => r.AddAsync(It.Is<Sector>(s =>
            s.Id != null && s.Id.Value != Guid.Empty), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "POST /api/sectors sets IsEnabled to true by default")]
    public async Task CreateSector_Should_SetIsEnabledToTrue_ByDefault()
    {
        // Arrange
        var createRequest = new CreateSectorRequest
        {
            Code = "SEC003",
            Name = "South Sector",
            CityId = Guid.NewGuid()
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/sectors", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        _sectorRepoMock.Verify(r => r.AddAsync(It.Is<Sector>(s =>
            s.IsEnabled == true), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory(DisplayName = "POST /api/sectors validates all required fields")]
    [InlineData("", "Name", "Code is required")]
    [InlineData("CODE", "", "Name is required")]
    [InlineData(null, "Name", "Code cannot be null")]
    [InlineData("CODE", null, "Name cannot be null")]
    public async Task CreateSector_Should_ReturnValidationError_WhenRequiredFieldsAreMissing(
        string code, string name, string scenario)
    {
        // Arrange
        var invalidRequest = new CreateSectorRequest
        {
            Code = code,
            Name = name,
            CityId = Guid.NewGuid()
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/sectors", invalidRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        _sectorRepoMock.Verify(r => r.AddAsync(It.IsAny<Sector>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "POST /api/sectors returns 400 when CityId is empty")]
    public async Task CreateSector_Should_ReturnValidationError_WhenCityIdIsEmpty()
    {
        // Arrange
        var invalidRequest = new CreateSectorRequest
        {
            Code = "SEC004",
            Name = "Test Sector",
            CityId = Guid.Empty
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/sectors", invalidRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        _sectorRepoMock.Verify(r => r.AddAsync(It.IsAny<Sector>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "POST /api/sectors returns 400 when City doesn't exist")]
    public async Task CreateSector_Should_ReturnValidationError_WhenCityDoesNotExist()
    {
        // Arrange
        var nonExistentCityId = Guid.NewGuid();
        _cityRepoMock.Setup(r => r.GetByIdAsync(CityId.Of(nonExistentCityId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((City)null);

        var invalidRequest = new CreateSectorRequest
        {
            Code = "SEC005",
            Name = "Test Sector",
            CityId = nonExistentCityId
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/sectors", invalidRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        _sectorRepoMock.Verify(r => r.AddAsync(It.IsAny<Sector>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory(DisplayName = "POST /api/sectors validates SectorCode format")]
    [InlineData("SEC-001", "Code with hyphen should be valid")]
    [InlineData("SEC_001", "Code with underscore should be valid")]
    [InlineData("SEC001", "Code without separator should be valid")]
    [InlineData("A", "Single character code should be valid")]
    [InlineData("VERYLONGCODETHATEXCEEDSNORMALLIMITS123456789", "Very long code should be handled")]
    public async Task CreateSector_Should_HandleVariousCodeFormats(string code, string scenario)
    {
        // Arrange
        var createRequest = new CreateSectorRequest
        {
            Code = code,
            Name = "Test Sector",
            CityId = Guid.NewGuid()
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/sectors", createRequest);

        // Assert
        if (code.Length > 0 && code.Length <= 50) // Assuming reasonable limits
        {
            response.StatusCode.Should().Be(HttpStatusCode.Created, because: scenario);
        }
        else
        {
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest, because: scenario);
        }
    }

    [Fact(DisplayName = "POST /api/sectors handles special characters in Name")]
    public async Task CreateSector_Should_HandleSpecialCharactersInName()
    {
        // Arrange
        var createRequest = new CreateSectorRequest
        {
            Code = "SEC006",
            Name = "Secteur VilleCentre et Peripherie Zone 1",
            CityId = Guid.NewGuid()
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/sectors", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        _sectorRepoMock.Verify(r => r.AddAsync(It.Is<Sector>(s =>
            s.Name == "Secteur VilleCentre et Peripherie Zone 1"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "POST /api/sectors returns proper error message for duplicate code")]
    public async Task CreateSector_Should_ReturnProperErrorMessage_WhenCodeAlreadyExists()
    {
        // Arrange
        var existingCode = "DUPLICATE_CODE";
        var existingSector = Sector.Create(
            SectorId.Of(Guid.NewGuid()),
            existingCode, "Existing Sector", CityId.Of(Guid.NewGuid()));

        _sectorRepoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Sector, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingSector);

        var duplicateRequest = new CreateSectorRequest
        {
            Code = existingCode,
            Name = "Another Sector",
            CityId = Guid.NewGuid()
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/sectors", duplicateRequest);
        var errorContent = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        errorContent.Should().Contain(existingCode);
    }
}