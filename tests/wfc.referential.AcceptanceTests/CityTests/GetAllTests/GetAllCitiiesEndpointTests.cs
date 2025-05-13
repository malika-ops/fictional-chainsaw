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
using wfc.referential.Application.Cities.Queries.GetAllCities;
using wfc.referential.Application.Interfaces;
using wfc.referential.Application.RegionManagement.Queries.GetAllRegions;
using wfc.referential.Domain.CityAggregate;
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.RegionAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.CityTests.GetAllTests;

public class GetAllCitiiesEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<ICityRepository> _repoMock = new();

    public GetAllCitiiesEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var customisedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<ICityRepository>();
                services.RemoveAll<ICacheService>();

                services.AddSingleton(_repoMock.Object);
                services.AddSingleton(cacheMock.Object);
            });
        });

        _client = customisedFactory.CreateClient();
    }

    // Helper to build dummy regions quickly
    private static City DummyCity(string code, string name) =>
        City.Create(CityId.Of(Guid.NewGuid()), code, name,"timezone","taxzone", RegionId.Of(Guid.NewGuid()), "abbrev");

    // Lightweight DTO for deserialising the endpoint response
    private record PagedResultDto<T>(T[] Items, int PageNumber, int PageSize,
                                     int TotalCount, int TotalPages);

    // 1) Happy‑path paging
    [Fact(DisplayName = "GET /api/cities returns paged list")]
    public async Task Get_ShouldReturnPagedList_WhenParamsAreValid()
    {
        // Arrange
        var allCities = new[] {DummyCity("NYC", "New York City"), DummyCity("LDN", "London"),
            DummyCity("PAR", "Paris"), DummyCity("TYO", "Tokyo"), DummyCity("TOR", "Toronto")};

        // repository returns first 2 items for page=1 size=2
        _repoMock.Setup(r => r.GetCitiesByCriteriaAsync(
                            It.Is<GetAllCitiesQuery>(q => q.PageNumber == 1 && q.PageSize == 2),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(allCities.Take(2).ToList());

        _repoMock.Setup(r => r.GetCountTotalAsync(
                            It.IsAny<GetAllCitiesQuery>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(allCities.Length);

        // Act
        var response = await _client.GetAsync("/api/cities?pageNumber=1&pageSize=2");
        var dto = await response.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(2);
        dto.TotalCount.Should().Be(5);
        dto.TotalPages.Should().Be(3);
        dto.PageNumber.Should().Be(1);
        dto.PageSize.Should().Be(2);

        _repoMock.Verify(r => r.GetCitiesByCriteriaAsync(
                                It.Is<GetAllCitiesQuery>(q => q.PageNumber == 1 && q.PageSize == 2),
                                It.IsAny<CancellationToken>()),
                         Times.Once);
    }

    // 2) Filter by code
    [Fact(DisplayName = "GET /api/cities?code=NYC returns only NYC zone")]
    public async Task Get_ShouldFilterByCode()
    {
        // Arrange
        var nyc = DummyCity("NYC", "US Dollar");

        _repoMock.Setup(r => r.GetCitiesByCriteriaAsync(
                            It.Is<GetAllCitiesQuery>(q => q.Code == "NYC"),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new List<City> { nyc });

        _repoMock.Setup(r => r.GetCountTotalAsync(
                            It.IsAny<GetAllCitiesQuery>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(1);

        // Act
        var response = await _client.GetAsync("/api/cities?code=NYC");
        var dto = await response.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(1);
        dto.Items[0].GetProperty("code").GetString().Should().Be("NYC");

        _repoMock.Verify(r => r.GetCitiesByCriteriaAsync(
                                It.Is<GetAllCitiesQuery>(q => q.Code == "NYC"),
                                It.IsAny<CancellationToken>()),
                         Times.Once);
    }

    ////// 3) Default paging when parameters are omitted
    [Fact(DisplayName = "GET /api/cities uses default paging when no query params supplied")]
    public async Task Get_ShouldUseDefaultPaging_WhenNoParamsProvided()
    {
        // Arrange
        // we’ll return 3 items – fewer than the default pageSize (10)
        var cities = new[] { DummyCity("NYC", "New York City"),
                            DummyCity("LDN", "London"),
                            DummyCity("PAR", "Paris")};

        _repoMock.Setup(r => r.GetCitiesByCriteriaAsync(
                            It.Is<GetAllCitiesQuery>(q => q.PageNumber == 1 && q.PageSize == 10),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(cities.ToList());

        _repoMock.Setup(r => r.GetCountTotalAsync(
                            It.IsAny<GetAllCitiesQuery>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(cities.Length);

        // Act
        var response = await _client.GetAsync("/api/cities");
        var dto = await response.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        dto!.PageNumber.Should().Be(1);
        dto.PageSize.Should().Be(10);
        dto.Items.Should().HaveCount(3);

        // repository must have been called with default paging values
        _repoMock.Verify(r => r.GetCitiesByCriteriaAsync(
                                It.Is<GetAllCitiesQuery>(q => q.PageNumber == 1 && q.PageSize == 10),
                                It.IsAny<CancellationToken>()),
                         Times.Once);
    }

}
