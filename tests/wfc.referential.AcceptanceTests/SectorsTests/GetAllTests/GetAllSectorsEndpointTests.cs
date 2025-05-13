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

    // Helper to build dummy sectors quickly
    private static Sector CreateTestSector(string code, string name)
    {
        var country = Country.Create(
            new CountryId(Guid.NewGuid()),
            "TC", "Test Country", "TC", "TC", "TCO", "+0", "0", false, false, 2,
            true,
            new Domain.MonetaryZoneAggregate.MonetaryZoneId(Guid.NewGuid())
        );

        var city = City.Create(
            new CityId(Guid.NewGuid()),
            "CITY1",
            "Test City",
            "GMT",
            "TZ",
            new Domain.RegionAggregate.RegionId(Guid.NewGuid()),
            "TC"
        );

        return Sector.Create(SectorId.Of(Guid.NewGuid()), code, name, city);
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
        _repoMock.Setup(r => r.GetFilteredSectorsAsync(
                            It.Is<GetAllSectorsQuery>(q => q.PageNumber == 1 && q.PageSize == 2),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(allSectors.Take(2).ToList());

        _repoMock.Setup(r => r.GetCountTotalAsync(
                            It.IsAny<GetAllSectorsQuery>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(allSectors.Length);

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

        _repoMock.Verify(r => r.GetFilteredSectorsAsync(
                                It.Is<GetAllSectorsQuery>(q => q.PageNumber == 1 && q.PageSize == 2),
                                It.IsAny<CancellationToken>()),
                         Times.Once);
    }

    [Fact(DisplayName = "GET /api/sectors?code=SECTOR1 returns only matching sector")]
    public async Task Get_ShouldFilterByCode()
    {
        // Arrange
        var sector = CreateTestSector("SECTOR1", "First Sector");

        _repoMock.Setup(r => r.GetFilteredSectorsAsync(
                            It.Is<GetAllSectorsQuery>(q => q.Code == "SECTOR1"),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new List<Sector> { sector });

        _repoMock.Setup(r => r.GetCountTotalAsync(
                            It.IsAny<GetAllSectorsQuery>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(1);

        // Act
        var response = await _client.GetAsync("/api/sectors?code=SECTOR1");
        var dto = await response.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(1);
        dto.Items[0].GetProperty("code").GetString().Should().Be("SECTOR1");

        _repoMock.Verify(r => r.GetFilteredSectorsAsync(
                                It.Is<GetAllSectorsQuery>(q => q.Code == "SECTOR1"),
                                It.IsAny<CancellationToken>()),
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

        _repoMock.Setup(r => r.GetFilteredSectorsAsync(
                            It.Is<GetAllSectorsQuery>(q => q.PageNumber == 1 && q.PageSize == 10),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(sectors.ToList());

        _repoMock.Setup(r => r.GetCountTotalAsync(
                            It.IsAny<GetAllSectorsQuery>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(sectors.Length);

        // Act
        var response = await _client.GetAsync("/api/sectors");
        var dto = await response.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        dto!.PageNumber.Should().Be(1);
        dto.PageSize.Should().Be(10);
        dto.Items.Should().HaveCount(3);

        // Repository must have been called with default paging values
        _repoMock.Verify(r => r.GetFilteredSectorsAsync(
                                It.Is<GetAllSectorsQuery>(q => q.PageNumber == 1 && q.PageSize == 10),
                                It.IsAny<CancellationToken>()),
                         Times.Once);
    }

    [Fact(DisplayName = "GET /api/sectors?isEnabled=false returns only disabled sectors")]
    public async Task Get_ShouldFilterByIsEnabled()
    {
        // Arrange
        var sector = CreateTestSector("SECTOR1", "First Sector");
        sector.Disable(); // Set IsEnabled to false

        _repoMock.Setup(r => r.GetFilteredSectorsAsync(
                            It.Is<GetAllSectorsQuery>(q => q.IsEnabled == false),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new List<Sector> { sector });

        _repoMock.Setup(r => r.GetCountTotalAsync(
                            It.IsAny<GetAllSectorsQuery>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(1);

        // Act
        var response = await _client.GetAsync("/api/sectors?isEnabled=false");
        var dto = await response.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(1);
        dto.Items[0].GetProperty("isEnabled").GetBoolean().Should().BeFalse();

        _repoMock.Verify(r => r.GetFilteredSectorsAsync(
                                It.Is<GetAllSectorsQuery>(q => q.IsEnabled == false),
                                It.IsAny<CancellationToken>()),
                         Times.Once);
    }
}