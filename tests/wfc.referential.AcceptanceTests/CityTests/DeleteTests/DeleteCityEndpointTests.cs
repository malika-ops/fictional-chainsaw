//using System.Net;
//using System.Net.Http.Json;
//using System.Text.Json;
//using BuildingBlocks.Application.Interfaces;
//using FluentAssertions;
//using Microsoft.AspNetCore.Hosting;
//using Microsoft.AspNetCore.Mvc.Testing;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.DependencyInjection.Extensions;
//using Moq;
//using wfc.referential.Application.Constants;
//using wfc.referential.Application.Interfaces;
//using wfc.referential.Domain.AgencyAggregate;
//using wfc.referential.Domain.CityAggregate;
//using wfc.referential.Domain.RegionAggregate;
//using Xunit;

//namespace wfc.referential.AcceptanceTests.CityTests.DeleteTests;

//public class DeleteCityEndpointTests : IClassFixture<WebApplicationFactory<Program>>
//{
//    private readonly HttpClient _client;
//    private readonly Mock<ICityRepository> _repoMock = new();
//    private readonly Mock<ICacheService> _cacheMock = new();
//    public DeleteCityEndpointTests(WebApplicationFactory<Program> factory)
//    {
//        // Clone the factory and customize the host
//        var customisedFactory = factory.WithWebHostBuilder(builder =>
//        {
//            builder.UseEnvironment("Testing");

//            builder.ConfigureServices(services =>
//            {
//                // 🧹 Remove concrete registrations that hit the DB / Redis
//                services.RemoveAll<ICityRepository>();
//                services.RemoveAll<ICacheService>();

//                // 🔌 Plug mocks back in
//                services.AddSingleton(_repoMock.Object);
//                services.AddSingleton(_cacheMock.Object);
//            });
//        });

//        _client = customisedFactory.CreateClient();
//    }


//    [Fact(DisplayName = "DELETE /api/cities/{id} returns 404 when city does not exist")]
//    public async Task Delete_ShouldReturn404_WhenCityDoesNotExist()
//    {
//        // Arrange
//        var cityId = Guid.NewGuid();
//        _repoMock.Setup(r => r.GetByIdAsync(cityId, It.IsAny<CancellationToken>()))
//            .ReturnsAsync((City)null); // City not found

//        // Act
//        var response = await _client.DeleteAsync($"/api/cities/{cityId}");

//        // Assert
//        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

//        // Et on ne doit pas appeler Patch ni Cache
//        _repoMock.Verify(r => r.UpdateCityAsync(It.IsAny<City>(), It.IsAny<CancellationToken>()), Times.Never);

//    }

//    [Fact(DisplayName = "DELETE /api/cities/{id} returns 400 when city is already assigned to an agency")]
//    public async Task Delete_ShouldReturn400_WhenCityIdHasAnAssignedAgency()
//    {
//        var cityId = Guid.NewGuid();
//        var city = City.Create(CityId.Of(cityId), "cityCode", "cityName", "TimeZone", RegionId.Of(Guid.NewGuid()), "aabre");

//        _repoMock
//            .Setup(c => c.GetByIdAsync(cityId, It.IsAny<CancellationToken>()))
//            .ReturnsAsync(city);

//        _repoMock
//            .Setup(c => c.HasAgencyAsync(city.Id, It.IsAny<CancellationToken>()))
//            .ReturnsAsync(true);

//        var response = await _client.DeleteAsync($"/api/cities/{cityId}");
//        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

//        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

//        doc.RootElement.GetProperty("errors").ToString()
//            .Should().Be($"{nameof(City)} with Id {cityId} already has an assigned {nameof(Agency)}.");

//        _repoMock.Verify(c => c.HasAgencyAsync(It.IsAny<CityId>(), It.IsAny<CancellationToken>()),
//            Times.Once);

//        _repoMock.Verify(c => c.UpdateCityAsync(It.IsAny<City>(), It.IsAny<CancellationToken>()),
//            Times.Never);
//    }

//    [Fact(DisplayName = "DELETE /api/cities/{id} returns 200 and disables city + clears cache")]
//    public async Task Delete_ShouldReturn200_WhenCityExists()
//    {
//        // Arrange
//        var cityId = Guid.NewGuid();
//        var city = City.Create(CityId.Of(cityId), "ABC", "TestCity", "UTC", RegionId.Of(Guid.NewGuid()), "TST");

//        _repoMock.Setup(r => r.GetByIdAsync(cityId, It.IsAny<CancellationToken>()))
//                 .ReturnsAsync(city);

//        _repoMock.Setup(r => r.UpdateCityAsync(It.IsAny<City>(), It.IsAny<CancellationToken>()))
//                 .Returns(Task.CompletedTask);
//        // Act
//        var response = await _client.DeleteAsync($"/api/cities/{cityId}");

//        // Assert
//        response.StatusCode.Should().Be(HttpStatusCode.OK);

//        _repoMock.Verify(r => r.UpdateCityAsync(It.Is<City>(c => c.IsEnabled == false), It.IsAny<CancellationToken>()), Times.Once);

//        _cacheMock.Verify(c => c.RemoveByPrefixAsync(CacheKeys.City.Prefix, It.IsAny<CancellationToken>()), Times.Once,
//            "Le cache des villes doit être invalidé après suppression");
//    }

//}
