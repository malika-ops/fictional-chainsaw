using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Moq;
using wfc.referential.Application.Constants;
using wfc.referential.Domain.AgencyAggregate;
using wfc.referential.Domain.CityAggregate;
using wfc.referential.Domain.CorridorAggregate;
using wfc.referential.Domain.RegionAggregate;
using wfc.referential.Domain.SectorAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.CityTests.DeleteTests;

public class DeleteCityEndpointTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{
    [Fact(DisplayName = "DELETE /api/cities/{id} returns 404 when city does not exist")]
    public async Task Delete_ShouldReturn404_WhenCityDoesNotExist()
    {
        // Arrange
        var cityId = Guid.NewGuid();
        _cityRepoMock.Setup(r => r.GetByIdAsync(CityId.Of(cityId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((City)null); // City not found

        // Act
        var response = await _client.DeleteAsync($"/api/cities/{cityId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // Et on ne doit pas appeler Patch ni Cache
        _cityRepoMock.Verify(r => r.Update(It.IsAny<City>()), Times.Never);
    }

    [Fact(DisplayName = "DELETE /api/cities/{id} returns 400 when city is already assigned to an agency")]
    public async Task Delete_ShouldReturn400_WhenCityIdHasAnAssignedAgency()
    {
        var cityId = CityId.Of(Guid.NewGuid());
        var city = City.Create(cityId, "cityCode", "cityName", "TimeZone", RegionId.Of(Guid.NewGuid()), "aabre");

        _cityRepoMock
            .Setup(c => c.GetByIdAsync(It.IsAny<CityId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(city);

        _agencyRepoMock
            .Setup(c => c.GetByConditionAsync(It.IsAny<Expression<Func<Agency, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Agency> {Agency.Create(
                id: AgencyId.Of(Guid.NewGuid()),
                code: "112233",
                name: "Existing",
                abbreviation: "EXI",
                address1: "addr",
                address2: null,
                phone: "000",
                fax: "",
                accountingSheetName: "sheet",
                accountingAccountNumber: "acc",
                postalCode: "10000",
                latitude: null,
                longitude: null,
                cashTransporter: null,
                expenseFundAccountingSheet: null,
                expenseFundAccountNumber: null,
                madAccount: null,
                fundingThreshold: null,
                cityId: CityId.Of(Guid.NewGuid()),
                sectorId: null,
                agencyTypeId: null,
                tokenUsageStatusId: null,
                fundingTypeId: null,
                partnerId: null,
                supportAccountId: null)});

        var response = await _client.DeleteAsync($"/api/cities/{cityId}");
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc.RootElement.GetProperty("errors")
                .GetProperty("message").ToString()
            .Should().Be($"{nameof(City)} with Id {cityId} already has an assigned {nameof(Agency)}.");

        _agencyRepoMock.Verify(c => c.GetByConditionAsync(It.IsAny<Expression<Func<Agency, bool>>>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _cityRepoMock.Verify(c => c.Update(It.IsAny<City>()),
            Times.Never);
    }

    [Fact(DisplayName = "DELETE /api/cities/{id} returns 200 and disables city")]
    public async Task Delete_ShouldReturn200_WhenCityExists()
    {
        // Arrange
        var cityId = CityId.Of(Guid.NewGuid());
        var city = City.Create(cityId, "ABC", "TestCity", "UTC", RegionId.Of(Guid.NewGuid()), "TST");

        _cityRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<CityId>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(city);

        _agencyRepoMock.Setup(r => r.GetByConditionAsync(It.IsAny<Expression<Func<Agency, bool>>>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(new List<Agency>());

        _sectorRepoMock.Setup(r => r.GetByConditionAsync(It.IsAny<Expression<Func<Sector, bool>>>(), It.IsAny<CancellationToken>()))
                             .ReturnsAsync(new List<Sector>());

        _corridorRepoMock.Setup(r => r.GetByConditionAsync(It.IsAny<Expression<Func<Corridor, bool>>>(), It.IsAny<CancellationToken>()))
                               .ReturnsAsync(new List<Corridor>());

        _cityRepoMock.Setup(r => r.Update(It.IsAny<City>()));
        _cityRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()));

        // Act
        var response = await _client.DeleteAsync($"/api/cities/{cityId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        _cityRepoMock.Verify(r => r.Update(It.Is<City>(c => c.IsEnabled == false)), Times.Once);
        _cityRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _cacheMock.Verify(c => c.RemoveByPrefixAsync(CacheKeys.City.Prefix, It.IsAny<CancellationToken>()), Times.Once);
    }
}