using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Moq;
using wfc.referential.Domain.AgencyAggregate;
using wfc.referential.Domain.CityAggregate;
using wfc.referential.Domain.SectorAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.AgencyTests.UpdateTests;

public class UpdateAgencyEndpointTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{
    private static Agency CreateAgency(Guid id, string code, string name, Guid? cityId = null, Guid? sectorId = null) =>
        Agency.Create(
            AgencyId.Of(id),
            code,
            name,
            "Abbrev",
            "Address1",
            null,
            "Phone",
            "Fax",
            "SheetName",
            "AccountNumber",
            "12345",
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            cityId.HasValue ? CityId.Of(cityId.Value) : null,
            sectorId.HasValue ? SectorId.Of(sectorId.Value) : null,
            null,
            null,
            null,
            null,
            null);


    [Fact(DisplayName = "PUT /api/agencies/{id} returns 404 when agency does not exist")]
    public async Task Put_ShouldReturn404_WhenAgencyNotFound()
    {
        // Arrange
        var agencyId = Guid.NewGuid();
        var cityId = Guid.NewGuid();

        _agencyRepoMock.Setup(r => r.GetByIdAsync(AgencyId.Of(agencyId), It.IsAny<CancellationToken>()))
                      .ReturnsAsync((Agency?)null);

        var payload = new
        {
            AgencyId = agencyId,
            Code = "123456",
            Name = "Test Agency",
            Abbreviation = "TST",
            Address1 = "Test Address",
            Phone = "123-456-7890",
            Fax = "123-456-7891",
            AccountingSheetName = "Sheet",
            AccountingAccountNumber = "ACC123",
            PostalCode = "12345",
            CityId = cityId
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/agencies/{agencyId}", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        _agencyRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PUT /api/agencies/{id} returns 409 when code already exists")]
    public async Task Put_ShouldReturn409_WhenCodeAlreadyExists()
    {
        // Arrange
        var agencyId = Guid.NewGuid();
        var duplicateAgencyId = Guid.NewGuid();
        var cityId = Guid.NewGuid();

        var existingAgency = CreateAgency(agencyId, "123456", "Original Agency", cityId);
        var duplicateAgency = CreateAgency(duplicateAgencyId, "654321", "Duplicate Agency", cityId);

        _agencyRepoMock.Setup(r => r.GetByIdAsync(AgencyId.Of(agencyId), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(existingAgency);

        _agencyRepoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Agency, bool>>>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(duplicateAgency); // Code already exists

        var payload = new
        {
            AgencyId = agencyId,
            Code = "654321", // This code already exists
            Name = "Updated Agency",
            Abbreviation = "UPD",
            Address1 = "New Address",
            Phone = "123-456-7890",
            Fax = "123-456-7891",
            AccountingSheetName = "New Sheet",
            AccountingAccountNumber = "ACC123",
            PostalCode = "54321",
            CityId = cityId
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/agencies/{agencyId}", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        _agencyRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PUT /api/agencies/{id} returns 400 when required fields are missing")]
    public async Task Put_ShouldReturn400_WhenRequiredFieldsMissing()
    {
        // Arrange
        var agencyId = Guid.NewGuid();
        var payload = new
        {
            AgencyId = agencyId,
            // Code omitted - required field
            Name = "Test Agency",
            // Other required fields omitted
            CityId = Guid.NewGuid()
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/agencies/{agencyId}", payload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors").EnumerateObject()
            .Should().Contain(prop => prop.Name.ToLower().Contains("code"));

        _agencyRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PUT /api/agencies/{id} returns 400 when code is not 6 digits")]
    public async Task Put_ShouldReturn400_WhenCodeIsNot6Digits()
    {
        // Arrange
        var agencyId = Guid.NewGuid();
        var payload = new
        {
            AgencyId = agencyId,
            Code = "12345", // Only 5 digits
            Name = "Test Agency",
            Abbreviation = "TST",
            Address1 = "Test Address",
            Phone = "123-456-7890",
            Fax = "123-456-7891",
            AccountingSheetName = "Sheet",
            AccountingAccountNumber = "ACC123",
            PostalCode = "12345",
            CityId = Guid.NewGuid()
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/agencies/{agencyId}", payload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var errorMessage = doc!.RootElement.GetProperty("errors").EnumerateObject()
            .FirstOrDefault(prop => prop.Name.ToLower().Contains("code"))
            .Value.EnumerateArray().FirstOrDefault().GetString();

        errorMessage.Should().Contain("6 digits");

        _agencyRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PUT /api/agencies/{id} returns 400 when both CityId and SectorId are provided")]
    public async Task Put_ShouldReturn400_WhenBothCityIdAndSectorIdProvided()
    {
        // Arrange
        var agencyId = Guid.NewGuid();
        var payload = new
        {
            AgencyId = agencyId,
            Code = "123456",
            Name = "Test Agency",
            Abbreviation = "TST",
            Address1 = "Test Address",
            Phone = "123-456-7890",
            Fax = "123-456-7891",
            AccountingSheetName = "Sheet",
            AccountingAccountNumber = "ACC123",
            PostalCode = "12345",
            CityId = Guid.NewGuid(),
            SectorId = Guid.NewGuid() // Both provided - should fail
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/agencies/{agencyId}", payload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var errorMessage = doc!.RootElement.GetProperty("errors").EnumerateObject()
            .FirstOrDefault().Value.EnumerateArray().FirstOrDefault().GetString();

        errorMessage.Should().Contain("Exactly one of CityId or SectorId");

        _agencyRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PUT /api/agencies/{id} returns 400 when neither CityId nor SectorId are provided")]
    public async Task Put_ShouldReturn400_WhenNeitherCityIdNorSectorIdProvided()
    {
        // Arrange
        var agencyId = Guid.NewGuid();
        var payload = new
        {
            AgencyId = agencyId,
            Code = "123456",
            Name = "Test Agency",
            Abbreviation = "TST",
            Address1 = "Test Address",
            Phone = "123-456-7890",
            Fax = "123-456-7891",
            AccountingSheetName = "Sheet",
            AccountingAccountNumber = "ACC123",
            PostalCode = "12345"
            // Neither CityId nor SectorId provided
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/agencies/{agencyId}", payload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var errorMessage = doc!.RootElement.GetProperty("errors").EnumerateObject()
            .FirstOrDefault().Value.EnumerateArray().FirstOrDefault().GetString();

        errorMessage.Should().Contain("Exactly one of CityId or SectorId");

        _agencyRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PUT /api/agencies/{id} returns 404 when referenced City does not exist")]
    public async Task Put_ShouldReturn404_WhenCityNotFound()
    {
        // Arrange
        var agencyId = Guid.NewGuid();
        var cityId = Guid.NewGuid();
        var existingAgency = CreateAgency(agencyId, "123456", "Test Agency", cityId);

        _agencyRepoMock.Setup(r => r.GetByIdAsync(AgencyId.Of(agencyId), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(existingAgency);

        _agencyRepoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Agency, bool>>>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync((Agency?)null);

        _cityRepoMock.Setup(r => r.GetByIdAsync(CityId.Of(cityId), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((wfc.referential.Domain.CityAggregate.City?)null); // City not found

        var payload = new
        {
            AgencyId = agencyId,
            Code = "654321",
            Name = "Updated Agency",
            Abbreviation = "UPD",
            Address1 = "New Address",
            Phone = "123-456-7890",
            Fax = "123-456-7891",
            AccountingSheetName = "New Sheet",
            AccountingAccountNumber = "ACC123",
            PostalCode = "54321",
            CityId = cityId
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/agencies/{agencyId}", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        _agencyRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PUT /api/agencies/{id} returns 404 when referenced Sector does not exist")]
    public async Task Put_ShouldReturn404_WhenSectorNotFound()
    {
        // Arrange
        var agencyId = Guid.NewGuid();
        var sectorId = Guid.NewGuid();
        var existingAgency = CreateAgency(agencyId, "123456", "Test Agency", null, sectorId);

        _agencyRepoMock.Setup(r => r.GetByIdAsync(AgencyId.Of(agencyId), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(existingAgency);

        _agencyRepoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Agency, bool>>>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync((Agency?)null);

        _sectorRepoMock.Setup(r => r.GetByIdAsync(SectorId.Of(sectorId), It.IsAny<CancellationToken>()))
                      .ReturnsAsync((wfc.referential.Domain.SectorAggregate.Sector?)null); // Sector not found

        var payload = new
        {
            AgencyId = agencyId,
            Code = "654321",
            Name = "Updated Agency",
            Abbreviation = "UPD",
            Address1 = "New Address",
            Phone = "123-456-7890",
            Fax = "123-456-7891",
            AccountingSheetName = "New Sheet",
            AccountingAccountNumber = "ACC123",
            PostalCode = "54321",
            SectorId = sectorId
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/agencies/{agencyId}", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        _agencyRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

}