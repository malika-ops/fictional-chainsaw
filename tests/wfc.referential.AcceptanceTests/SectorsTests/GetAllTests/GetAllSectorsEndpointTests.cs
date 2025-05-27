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
using wfc.referential.Application.Sectors.Queries.GetAllSectors;
using wfc.referential.Domain.CityAggregate;
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.CurrencyAggregate;
using wfc.referential.Domain.SectorAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.SectorsTests.GetAllTests;

public class GetAllSectorsEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<ISectorRepository> _repoMock = new();

    public GetAllSectorsEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var customisedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<ISectorRepository>();
                services.RemoveAll<ICacheService>();

                services.AddSingleton(_repoMock.Object);
                services.AddSingleton(cacheMock.Object);
            });
        });

        _client = customisedFactory.CreateClient();
    }

    // Helper to build dummy sectors quickly - Fixed with abbreviation parameter
    private static Sector CreateTestSector(string code, string name, bool isEnabled = true)
    {
        var country = Country.Create(
            new CountryId(Guid.NewGuid()),
            "TC", "Test Country", "TC", "TC", "TCO", "+0", "0", false, false, 2,
            true,
            new Domain.MonetaryZoneAggregate.MonetaryZoneId(Guid.NewGuid()),
            new CurrencyId(Guid.NewGuid())
        );

        var city = City.Create(
            new CityId(Guid.NewGuid()),
            "CITY1",
            "Test City",
            "GMT",
            new Domain.RegionAggregate.RegionId(Guid.NewGuid()),
            "TC" // Added abbreviation parameter
        );

        var sector = Sector.Create(SectorId.Of(Guid.NewGuid()), code, name, city.Id);
        if (!isEnabled)
        {
            sector.Disable();
        }
        return sector;
    }

    // Lightweight DTO for deserialising the endpoint response
    private record PagedResultDto<T>(T[] Items, int PageNumber, int PageSize,
                                     int TotalCount, int TotalPages);

    [Fact(DisplayName = "GET /api/sectors returns paged list")]
    public async Task Get_ShouldReturnPagedList_WhenParamsAreValid()
    {
        // Arrange
        var allSectors = new[] {
            CreateTestSector("SECTOR1", "First Sector"),
            CreateTestSector("SECTOR2", "Second Sector"),
            CreateTestSector("SECTOR3", "Third Sector"),
            CreateTestSector("SECTOR4", "Fourth Sector"),
            CreateTestSector("SECTOR5", "Fifth Sector")
        };

        // Repository returns first 2 items for page=1 size=2
        _repoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.Is<GetAllSectorsQuery>(q => q.PageNumber == 1 && q.PageSize == 2),
                            1, 2, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new BuildingBlocks.Core.Pagination.PagedResult<Sector>(
                     allSectors.Take(2).ToList(), allSectors.Length, 1, 2));

        // Act
        var response = await _client.GetAsync("/api/sectors?pageNumber=1&pageSize=2");
        var dto = await response.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(2);
        dto.TotalCount.Should().Be(5);
        dto.TotalPages.Should().Be(3);
        dto.PageNumber.Should().Be(1);
        dto.PageSize.Should().Be(2);

        _repoMock.Verify(r => r.GetPagedByCriteriaAsync(
                                It.Is<GetAllSectorsQuery>(q => q.PageNumber == 1 && q.PageSize == 2),
                                1, 2, It.IsAny<CancellationToken>()),
                         Times.Once);
    }

    [Fact(DisplayName = "GET /api/sectors?code=SECTOR1 returns only matching sector")]
    public async Task Get_ShouldFilterByCode()
    {
        // Arrange
        var sector = CreateTestSector("SECTOR1", "First Sector");

        _repoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.Is<GetAllSectorsQuery>(q => q.Code == "SECTOR1"),
                            It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new BuildingBlocks.Core.Pagination.PagedResult<Sector>(
                     new List<Sector> { sector }, 1, 1, 10));

        // Act
        var response = await _client.GetAsync("/api/sectors?code=SECTOR1");
        var dto = await response.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(1);
        dto.Items[0].GetProperty("code").GetString().Should().Be("SECTOR1");

        _repoMock.Verify(r => r.GetPagedByCriteriaAsync(
                                It.Is<GetAllSectorsQuery>(q => q.Code == "SECTOR1"),
                                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
                         Times.Once);
    }

    [Fact(DisplayName = "GET /api/sectors uses default paging when no query params supplied")]
    public async Task Get_ShouldUseDefaultPaging_WhenNoParamsProvided()
    {
        // Arrange
        // We'll return 3 items – fewer than the default pageSize (10)
        var sectors = new[] {
            CreateTestSector("SECTOR1", "First Sector"),
            CreateTestSector("SECTOR2", "Second Sector"),
            CreateTestSector("SECTOR3", "Third Sector")
        };

        _repoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.Is<GetAllSectorsQuery>(q => q.PageNumber == 1 && q.PageSize == 10),
                            1, 10, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new BuildingBlocks.Core.Pagination.PagedResult<Sector>(
                     sectors.ToList(), sectors.Length, 1, 10));

        // Act
        var response = await _client.GetAsync("/api/sectors");
        var dto = await response.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        dto!.PageNumber.Should().Be(1);
        dto.PageSize.Should().Be(10);
        dto.Items.Should().HaveCount(3);

        // Repository must have been called with default paging values
        _repoMock.Verify(r => r.GetPagedByCriteriaAsync(
                                It.Is<GetAllSectorsQuery>(q => q.PageNumber == 1 && q.PageSize == 10),
                                1, 10, It.IsAny<CancellationToken>()),
                         Times.Once);
    }

    [Fact(DisplayName = "GET /api/sectors?isEnabled=false returns only disabled sectors")]
    public async Task Get_ShouldFilterByIsEnabled()
    {
        // Arrange
        var sector = CreateTestSector("SECTOR1", "First Sector", false); // Disabled

        _repoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.Is<GetAllSectorsQuery>(q => q.IsEnabled == false),
                            It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new BuildingBlocks.Core.Pagination.PagedResult<Sector>(
                     new List<Sector> { sector }, 1, 1, 10));

        // Act
        var response = await _client.GetAsync("/api/sectors?isEnabled=false");
        var dto = await response.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(1);
        dto.Items[0].GetProperty("isEnabled").GetBoolean().Should().BeFalse();

        _repoMock.Verify(r => r.GetPagedByCriteriaAsync(
                                It.Is<GetAllSectorsQuery>(q => q.IsEnabled == false),
                                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
                         Times.Once);
    }

    [Fact(DisplayName = "GET /api/sectors?name=Test returns sectors with matching name")]
    public async Task Get_ShouldFilterByName()
    {
        // Arrange
        var sectors = new[] {
            CreateTestSector("SECTOR1", "Test Sector Alpha"),
            CreateTestSector("SECTOR2", "Test Sector Beta")
        };

        _repoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.Is<GetAllSectorsQuery>(q => q.Name == "Test"),
                            It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new BuildingBlocks.Core.Pagination.PagedResult<Sector>(
                     sectors.ToList(), sectors.Length, 1, 10));

        // Act
        var response = await _client.GetAsync("/api/sectors?name=Test");
        var dto = await response.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(2);
        dto.Items.Should().AllSatisfy(item =>
            item.GetProperty("name").GetString()!.Should().Contain("Test"));
    }

    [Fact(DisplayName = "GET /api/sectors?cityId={guid} returns sectors for specific city")]
    public async Task Get_ShouldFilterByCityId()
    {
        // Arrange
        var cityId = Guid.NewGuid();
        var sectors = new[] {
            CreateTestSector("SECTOR1", "City Sector 1"),
            CreateTestSector("SECTOR2", "City Sector 2")
        };

        _repoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.Is<GetAllSectorsQuery>(q => q.CityId == cityId),
                            It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new BuildingBlocks.Core.Pagination.PagedResult<Sector>(
                     sectors.ToList(), sectors.Length, 1, 10));

        // Act
        var response = await _client.GetAsync($"/api/sectors?cityId={cityId}");
        var dto = await response.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(2);
    }

    [Fact(DisplayName = "GET /api/sectors returns empty result when no sectors match filters")]
    public async Task Get_ShouldReturnEmptyResult_WhenNoSectorsMatchFilters()
    {
        // Arrange
        _repoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.Is<GetAllSectorsQuery>(q => q.Code == "NONEXISTENT"),
                            It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new BuildingBlocks.Core.Pagination.PagedResult<Sector>(
                     new List<Sector>(), 0, 1, 10));

        // Act
        var response = await _client.GetAsync("/api/sectors?code=NONEXISTENT");
        var dto = await response.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().BeEmpty();
        dto.TotalCount.Should().Be(0);
        dto.TotalPages.Should().Be(0);
    }

    [Fact(DisplayName = "GET /api/sectors supports multiple filter combinations")]
    public async Task Get_ShouldSupportMultipleFilters_WhenCombined()
    {
        // Arrange
        var cityId = Guid.NewGuid();
        var sector = CreateTestSector("ACTIVE_CODE", "Active Sector");

        _repoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.Is<GetAllSectorsQuery>(q =>
                                q.Code == "ACTIVE_CODE" &&
                                q.Name == "Active" &&
                                q.CityId == cityId &&
                                q.IsEnabled == true),
                            It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new BuildingBlocks.Core.Pagination.PagedResult<Sector>(
                     new List<Sector> { sector }, 1, 1, 10));

        // Act
        var response = await _client.GetAsync($"/api/sectors?code=ACTIVE_CODE&name=Active&cityId={cityId}&isEnabled=true");
        var dto = await response.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(1);
        dto.Items[0].GetProperty("code").GetString().Should().Be("ACTIVE_CODE");
    }

    [Theory(DisplayName = "GET /api/sectors handles various page sizes")]
    [InlineData(1, "Single item per page")]
    [InlineData(5, "Small page size")]
    [InlineData(50, "Large page size")]
    [InlineData(100, "Very large page size")]
    public async Task Get_ShouldHandleVariousPageSizes(int pageSize, string scenario)
    {
        // Arrange
        var sectors = Enumerable.Range(1, Math.Min(pageSize, 10))
            .Select(i => CreateTestSector($"SECTOR{i:000}", $"Sector {i}"))
            .ToList();

        _repoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.Is<GetAllSectorsQuery>(q => q.PageSize == pageSize),
                            It.IsAny<int>(), pageSize, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new BuildingBlocks.Core.Pagination.PagedResult<Sector>(
                     sectors, sectors.Count, 1, pageSize));

        // Act
        var response = await _client.GetAsync($"/api/sectors?pageSize={pageSize}");
        var dto = await response.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, because: scenario);
        dto!.PageSize.Should().Be(pageSize);
        dto.Items.Length.Should().BeLessThanOrEqualTo(pageSize);
    }

    [Fact(DisplayName = "GET /api/sectors calculates correct total pages")]
    public async Task Get_ShouldCalculateCorrectTotalPages_ForGivenTotalCount()
    {
        // Arrange - 23 total items, page size 5 should give 5 pages
        var sectors = Enumerable.Range(1, 5)
            .Select(i => CreateTestSector($"SECTOR{i:000}", $"Sector {i}"))
            .ToList();

        _repoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.IsAny<GetAllSectorsQuery>(),
                            It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new BuildingBlocks.Core.Pagination.PagedResult<Sector>(
                     sectors, 23, 1, 5)); // 23 total items, page 1, size 5

        // Act
        var response = await _client.GetAsync("/api/sectors?pageSize=5");
        var dto = await response.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.TotalCount.Should().Be(23);
        dto.TotalPages.Should().Be(5); // Ceiling(23/5) = 5
        dto.PageSize.Should().Be(5);
        dto.PageNumber.Should().Be(1);
    }
}