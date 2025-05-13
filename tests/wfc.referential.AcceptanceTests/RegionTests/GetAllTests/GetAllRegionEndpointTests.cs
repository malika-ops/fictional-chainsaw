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
using wfc.referential.Application.RegionManagement.Queries.GetAllRegions;
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.RegionAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.RegionTests.GetAllTests;

public class GetAllRegionEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IRegionRepository> _repoMock = new();

    public GetAllRegionEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var customisedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IRegionRepository>();
                services.RemoveAll<ICacheService>();

                services.AddSingleton(_repoMock.Object);
                services.AddSingleton(cacheMock.Object);
            });
        });

        _client = customisedFactory.CreateClient();
    }

    // Helper to build dummy regions quickly
    private static Region DummyRegion(string code, string name) =>
        Region.Create(RegionId.Of(Guid.NewGuid()), code, name, CountryId.Of(Guid.NewGuid()));

    // Lightweight DTO for deserialising the endpoint response
    private record PagedResultDto<T>(T[] Items, int PageNumber, int PageSize,
                                     int TotalCount, int TotalPages);

    // 1) Happy‑path paging
    [Fact(DisplayName = "GET /api/regions returns paged list")]
    public async Task Get_ShouldReturnPagedList_WhenParamsAreValid()
    {
        // Arrange
        var allRegions = new[] { DummyRegion("USD", "US Dollar"), DummyRegion("EUR", "Euro"),
                               DummyRegion("GBP", "Pound"),    DummyRegion("JPY", "Yen"),
                               DummyRegion("CAD", "Canadian") };

        // repository returns first 2 items for page=1 size=2
        _repoMock.Setup(r => r.GetRegionsByCriteriaAsync(
                            It.Is<GetAllRegionsQuery>(q => q.PageNumber == 1 && q.PageSize == 2),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(allRegions.Take(2).ToList());

        _repoMock.Setup(r => r.GetCountTotalAsync(
                            It.IsAny<GetAllRegionsQuery>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(allRegions.Length);

        // Act
        var response = await _client.GetAsync("/api/regions?pageNumber=1&pageSize=2");
        var dto = await response.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(2);
        dto.TotalCount.Should().Be(5);
        dto.TotalPages.Should().Be(3);
        dto.PageNumber.Should().Be(1);
        dto.PageSize.Should().Be(2);

        _repoMock.Verify(r => r.GetRegionsByCriteriaAsync(
                                It.Is<GetAllRegionsQuery>(q => q.PageNumber == 1 && q.PageSize == 2),
                                It.IsAny<CancellationToken>()),
                         Times.Once);
    }

    // 2) Filter by code
    [Fact(DisplayName = "GET /api/monetaryZones?code=USD returns only USD zone")]
    public async Task Get_ShouldFilterByCode()
    {
        // Arrange
        var usd = DummyRegion("USD", "US Dollar");

        _repoMock.Setup(r => r.GetRegionsByCriteriaAsync(
                            It.Is<GetAllRegionsQuery>(q => q.Code == "USD"),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new List<Region> { usd });

        _repoMock.Setup(r => r.GetCountTotalAsync(
                            It.IsAny<GetAllRegionsQuery>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(1);

        // Act
        var response = await _client.GetAsync("/api/regions?code=USD");
        var dto = await response.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(1);
        dto.Items[0].GetProperty("code").GetString().Should().Be("USD");

        _repoMock.Verify(r => r.GetRegionsByCriteriaAsync(
                                It.Is<GetAllRegionsQuery>(q => q.Code == "USD"),
                                It.IsAny<CancellationToken>()),
                         Times.Once);
    }

    //// 3) Default paging when parameters are omitted
    [Fact(DisplayName = "GET /api/monetaryZones uses default paging when no query params supplied")]
    public async Task Get_ShouldUseDefaultPaging_WhenNoParamsProvided()
    {
        // Arrange
        // we’ll return 3 items – fewer than the default pageSize (10)
        var zones = new[] { DummyRegion("USD", "US Dollar"),
                        DummyRegion("EUR", "Euro"),
                        DummyRegion("GBP", "Pound") };

        _repoMock.Setup(r => r.GetRegionsByCriteriaAsync(
                            It.Is<GetAllRegionsQuery>(q => q.PageNumber == 1 && q.PageSize == 10),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(zones.ToList());

        _repoMock.Setup(r => r.GetCountTotalAsync(
                            It.IsAny<GetAllRegionsQuery>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(zones.Length);

        // Act
        var response = await _client.GetAsync("/api/regions");
        var dto = await response.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        dto!.PageNumber.Should().Be(1);
        dto.PageSize.Should().Be(10);
        dto.Items.Should().HaveCount(3);

        // repository must have been called with default paging values
        _repoMock.Verify(r => r.GetRegionsByCriteriaAsync(
                                It.Is<GetAllRegionsQuery>(q => q.PageNumber == 1 && q.PageSize == 10),
                                It.IsAny<CancellationToken>()),
                         Times.Once);
    }

}
