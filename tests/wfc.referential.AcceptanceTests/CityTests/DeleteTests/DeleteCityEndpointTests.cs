using System.Linq.Expressions;
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
using wfc.referential.Application.Constants;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.AgencyAggregate;
using wfc.referential.Domain.CityAggregate;
using wfc.referential.Domain.RegionAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.CityTests.DeleteTests;

public class DeleteCityEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<ICityRepository> _repoMock = new();
    private readonly Mock<IAgencyRepository> _repoAgencyMock = new();
    private readonly Mock<ICacheService> _cacheMock = new();
    public DeleteCityEndpointTests(WebApplicationFactory<Program> factory)
    {
        // Clone the factory and customize the host
        var customisedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                // 🧹 Remove concrete registrations that hit the DB / Redis
                services.RemoveAll<ICityRepository>();
                services.RemoveAll<IAgencyRepository>();
                services.RemoveAll<ICacheService>();

                // 🔌 Plug mocks back in
                services.AddSingleton(_repoMock.Object);
                services.AddSingleton(_repoAgencyMock.Object);
                services.AddSingleton(_cacheMock.Object);
            });
        });

        _client = customisedFactory.CreateClient();
    }


    [Fact(DisplayName = "DELETE /api/cities/{id} returns 404 when city does not exist")]
    public async Task Delete_ShouldReturn404_WhenCityDoesNotExist()
    {
        // Arrange
        var cityId = Guid.NewGuid();
        _repoMock.Setup(r => r.GetByIdAsync(CityId.Of(cityId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((City)null); // City not found

        // Act
        var response = await _client.DeleteAsync($"/api/cities/{cityId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // Et on ne doit pas appeler Patch ni Cache
        _repoMock.Verify(r => r.Update(It.IsAny<City>()), Times.Never);

    }

    [Fact(DisplayName = "DELETE /api/cities/{id} returns 400 when city is already assigned to an agency")]
    public async Task Delete_ShouldReturn400_WhenCityIdHasAnAssignedAgency()
    {
        var cityId = CityId.Of(Guid.NewGuid());
        var city = City.Create(cityId, "cityCode", "cityName", "TimeZone", RegionId.Of(Guid.NewGuid()), "aabre");

        _repoMock
            .Setup(c => c.GetByIdAsync(It.IsAny<CityId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(city);

        _repoAgencyMock
            .Setup(c => c.GetByConditionAsync(It.IsAny<Expression<Func<Agency, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Agency> {Agency.Create(
                AgencyId.Of(Guid.NewGuid()), "AgenctName", "Old Agency", "OLD",
                "addr", null, "phone", "", "sheet", "acc", "", "", "10000", "",
                null, null,  null, null, null, null, null)});

        var response = await _client.DeleteAsync($"/api/cities/{cityId}");
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc.RootElement.GetProperty("errors").ToString()
            .Should().Be($"{nameof(City)} with Id {cityId} already has an assigned {nameof(Agency)}.");

        _repoAgencyMock.Verify(c => c.GetByConditionAsync(It.IsAny<Expression<Func<Agency, bool>>>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _repoMock.Verify(c => c.Update(It.IsAny<City>()),
            Times.Never);
    }

    [Fact(DisplayName = "DELETE /api/cities/{id} returns 200 and disables city")]
    public async Task Delete_ShouldReturn200_WhenCityExists()
    {
        // Arrange
        var cityId = CityId.Of(Guid.NewGuid());
        var city = City.Create(cityId, "ABC", "TestCity", "UTC", RegionId.Of(Guid.NewGuid()), "TST");

        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<CityId>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(city);

        _repoMock.Setup(r => r.Update(It.IsAny<City>()));
        // Act
        var response = await _client.DeleteAsync($"/api/cities/{cityId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        _repoMock.Verify(r => r.Update(It.Is<City>(c => c.IsEnabled == false)), Times.Once);


        _cacheMock.Verify(c => c.RemoveByPrefixAsync(CacheKeys.City.Prefix, It.IsAny<CancellationToken>()), Times.Once);
    }

}
