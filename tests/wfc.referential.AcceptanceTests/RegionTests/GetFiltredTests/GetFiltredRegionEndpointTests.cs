using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Pagination;
using Castle.Components.DictionaryAdapter.Xml;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using wfc.referential.Application.Interfaces;
using wfc.referential.Application.RegionManagement.Queries.GetFiltredRegions;
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.RegionAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.RegionTests.GetFiltredTests;

public class GetFiltredRegionEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IRegionRepository> _repoMock = new();

    public GetFiltredRegionEndpointTests(WebApplicationFactory<Program> factory)
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
        var allRegions = new[] { DummyRegion("CASA", "US Dollar"), DummyRegion("EUR", "Euro"),
                               DummyRegion("GBP", "Pound"),    DummyRegion("JPY", "Yen"),
                               DummyRegion("CAD", "Canadian") };

        // repository returns first 2 items for page=1 size=2
        _repoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.Is<GetFiltredRegionsQuery>(q => q.PageNumber == 1 && q.PageSize == 2),
                            1, 2,
                            It.IsAny<CancellationToken>(), r => r.Cities))
                 .ReturnsAsync(new PagedResult<Region>(allRegions.Take(2).ToList(), 5, 1, 2));

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

        _repoMock.Verify(r => r.GetPagedByCriteriaAsync(
                                It.Is<GetFiltredRegionsQuery>(q => q.PageNumber == 1 && q.PageSize == 2),
                                1, 2,
                                It.IsAny<CancellationToken>(), r => r.Cities),
                         Times.Once);
    }

    // 2) Filter by code
    [Fact(DisplayName = "GET /api/regions?code=Casa returns only Casa zone")]
    public async Task Get_ShouldFilterByCode()
    {
        // Arrange
        var usd = DummyRegion("CASA", "US Dollar");

        _repoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.Is<GetFiltredRegionsQuery>(q => q.Code == "CASA"),1,10,
                            It.IsAny<CancellationToken>(), r => r.Cities))
                 .ReturnsAsync(new PagedResult<Region>(new List<Region> { usd }, 1,1, 10));
        // Act
        var response = await _client.GetAsync("/api/regions?code=CASA");
        var dto = await response.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(1);
        dto.Items[0].GetProperty("code").GetString().Should().Be("CASA");

        _repoMock.Verify(r => r.GetPagedByCriteriaAsync(
                                It.Is<GetFiltredRegionsQuery>(q => q.Code == "CASA"),
                                1, 10,
                                It.IsAny<CancellationToken>(), r => r.Cities),
                         Times.Once);
    }

    //// 3) Default paging when parameters are omitted
    [Fact(DisplayName = "GET /api/regions uses default paging when no query params supplied")]
    public async Task Get_ShouldUseDefaultPaging_WhenNoParamsProvided()
    {
        // Arrange
        // we’ll return 3 items – fewer than the default pageSize (10)
        var zones = new[] { DummyRegion("CASA", "US Dollar"),
                        DummyRegion("EUR", "Euro"),
                        DummyRegion("GBP", "Pound") };

        _repoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.Is<GetFiltredRegionsQuery>(q => q.PageNumber == 1 && q.PageSize == 10),
                            1, 10,
        It.IsAny<CancellationToken>(), r => r.Cities))
                 .ReturnsAsync(new PagedResult<Region>(zones.ToList(), zones.Count(), 1, 10));

        // Act
        var response = await _client.GetAsync("/api/regions");
        var dto = await response.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        dto!.PageNumber.Should().Be(1);
        dto.PageSize.Should().Be(10);
        dto.Items.Should().HaveCount(3);

        // repository must have been called with default paging values
        _repoMock.Verify(r => r.GetPagedByCriteriaAsync(
                                It.Is<GetFiltredRegionsQuery>(q => q.PageNumber == 1 && q.PageSize == 10),
                                1, 10,
                                It.IsAny<CancellationToken>(), r => r.Cities),
                         Times.Once);
    }

}
