using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Pagination;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using wfc.referential.Application.Interfaces;
using wfc.referential.Application.Tiers.Queries.GetAllTiers;
using wfc.referential.Domain.TierAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.TierTests.GetAllTests;

public class GetAllTiersEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<ITierRepository> _repoMock = new();

    public GetAllTiersEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var customisedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<ITierRepository>();
                services.RemoveAll<ICacheService>();

                services.AddSingleton(_repoMock.Object);
                services.AddSingleton(cacheMock.Object);
            });
        });

        _client = customisedFactory.CreateClient();
    }

    private static Tier CreateTier(string name, string description, bool isEnabled = true) =>
        Tier.Create(TierId.Of(Guid.NewGuid()), name, description);

    private record PagedResultDto<T>(T[] Items, int PageNumber, int PageSize,
                                     int TotalCount, int TotalPages);

    [Fact(DisplayName = "GET /api/tiers returns paged list")]
    public async Task Get_ShouldReturnPagedList_WhenParamsAreValid()
    {
        // Arrange
        var allTiers = new[]
        {
            CreateTier("Bronze", "Bronze tier"),
            CreateTier("Silver", "Silver tier"),
            CreateTier("Gold", "Gold tier"),
            CreateTier("Platinum", "Platinum tier"),
            CreateTier("Diamond", "Diamond tier")
        };

        var pagedResult = new PagedResult<Tier>(
            allTiers.Take(2).ToList(),
            allTiers.Length,
            1,
            2);

        _repoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.Is<GetAllTiersQuery>(q => q.PageNumber == 1 && q.PageSize == 2),
                            1,
                            2,
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(pagedResult);

        // Act
        var response = await _client.GetAsync("/api/tiers?pageNumber=1&pageSize=2");
        var dto = await response.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(2);
        dto.TotalCount.Should().Be(5);
        dto.TotalPages.Should().Be(3);
        dto.PageNumber.Should().Be(1);
        dto.PageSize.Should().Be(2);

        _repoMock.Verify(r => r.GetPagedByCriteriaAsync(
                                It.Is<GetAllTiersQuery>(q => q.PageNumber == 1 && q.PageSize == 2),
                                1,
                                2,
                                It.IsAny<CancellationToken>()),
                         Times.Once);
    }

    [Fact(DisplayName = "GET /api/tiers?name=Bronze returns only Bronze tier")]
    public async Task Get_ShouldFilterByName()
    {
        // Arrange
        var bronzeTier = CreateTier("Bronze", "Bronze tier");

        var pagedResult = new PagedResult<Tier>(
            new List<Tier> { bronzeTier },
            1,
            1,
            10);

        _repoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.Is<GetAllTiersQuery>(q => q.Name == "Bronze"),
                            1,
                            10,
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(pagedResult);

        // Act
        var response = await _client.GetAsync("/api/tiers?name=Bronze");
        var dto = await response.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(1);
        dto.Items[0].GetProperty("name").GetString().Should().Be("Bronze");

        _repoMock.Verify(r => r.GetPagedByCriteriaAsync(
                                It.Is<GetAllTiersQuery>(q => q.Name == "Bronze"),
                                1,
                                10,
                                It.IsAny<CancellationToken>()),
                         Times.Once);
    }

    [Fact(DisplayName = "GET /api/tiers?description=Silver returns tiers with Silver description")]
    public async Task Get_ShouldFilterByDescription()
    {
        // Arrange
        var silverTier = CreateTier("Silver", "Silver tier description");

        var pagedResult = new PagedResult<Tier>(
            new List<Tier> { silverTier },
            1,
            1,
            10);

        _repoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.Is<GetAllTiersQuery>(q => q.Description == "Silver"),
                            1,
                            10,
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(pagedResult);

        // Act
        var response = await _client.GetAsync("/api/tiers?description=Silver");
        var dto = await response.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(1);

        _repoMock.Verify(r => r.GetPagedByCriteriaAsync(
                                It.Is<GetAllTiersQuery>(q => q.Description == "Silver"),
                                1,
                                10,
                                It.IsAny<CancellationToken>()),
                         Times.Once);
    }

    [Fact(DisplayName = "GET /api/tiers?isEnabled=false returns only disabled tiers")]
    public async Task Get_ShouldFilterByEnabledStatus()
    {
        // Arrange
        var disabledTier = CreateTier("Disabled", "Disabled tier");
        disabledTier.Disable(); // Assuming this method exists to disable the tier

        var pagedResult = new PagedResult<Tier>(
            new List<Tier> { disabledTier },
            1,
            1,
            10);

        _repoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.Is<GetAllTiersQuery>(q => q.IsEnabled == false),
                            1,
                            10,
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(pagedResult);

        // Act
        var response = await _client.GetAsync("/api/tiers?isEnabled=false");
        var dto = await response.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(1);
        dto.Items[0].GetProperty("isEnabled").GetBoolean().Should().BeFalse();

        _repoMock.Verify(r => r.GetPagedByCriteriaAsync(
                                It.Is<GetAllTiersQuery>(q => q.IsEnabled == false),
                                1,
                                10,
                                It.IsAny<CancellationToken>()),
                         Times.Once);
    }

    [Fact(DisplayName = "GET /api/tiers uses default paging when no query params supplied")]
    public async Task Get_ShouldUseDefaultPaging_WhenNoParamsProvided()
    {
        // Arrange
        // we'll return 3 items – fewer than the default pageSize (10)
        var tiers = new[]
        {
            CreateTier("Bronze", "Bronze tier"),
            CreateTier("Silver", "Silver tier"),
            CreateTier("Gold", "Gold tier")
        };

        var pagedResult = new PagedResult<Tier>(
            tiers.ToList(),
            tiers.Length,
            1,
            10);

        _repoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.Is<GetAllTiersQuery>(q => q.PageNumber == 1 && q.PageSize == 10 && q.IsEnabled == true),
                            1,
                            10,
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(pagedResult);

        // Act
        var response = await _client.GetAsync("/api/tiers");
        var dto = await response.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        dto!.PageNumber.Should().Be(1);
        dto.PageSize.Should().Be(10);
        dto.Items.Should().HaveCount(3);

        // repository must have been called with default paging values
        _repoMock.Verify(r => r.GetPagedByCriteriaAsync(
                                It.Is<GetAllTiersQuery>(q => q.PageNumber == 1 && q.PageSize == 10 && q.IsEnabled == true),
                                1,
                                10,
                                It.IsAny<CancellationToken>()),
                         Times.Once);
    }

    [Fact(DisplayName = "GET /api/tiers with multiple filters works correctly")]
    public async Task Get_ShouldApplyMultipleFilters()
    {
        // Arrange
        var goldTier = CreateTier("Gold", "Premium gold tier");

        var pagedResult = new PagedResult<Tier>(
            new List<Tier> { goldTier },
            1,
            1,
            10);

        _repoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.Is<GetAllTiersQuery>(q => q.Name == "Gold" &&
                                                       q.Description == "Premium" &&
                                                       q.IsEnabled == true),
                            1,
                            10,
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(pagedResult);

        // Act
        var response = await _client.GetAsync("/api/tiers?name=Gold&description=Premium&isEnabled=true");
        var dto = await response.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(1);
        dto.Items[0].GetProperty("name").GetString().Should().Be("Gold");

        _repoMock.Verify(r => r.GetPagedByCriteriaAsync(
                                It.Is<GetAllTiersQuery>(q => q.Name == "Gold" &&
                                                           q.Description == "Premium" &&
                                                           q.IsEnabled == true),
                                1,
                                10,
                                It.IsAny<CancellationToken>()),
                         Times.Once);
    }

    [Fact(DisplayName = "GET /api/tiers returns empty list when no tiers match criteria")]
    public async Task Get_ShouldReturnEmptyList_WhenNoTiersMatchCriteria()
    {
        // Arrange
        var pagedResult = new PagedResult<Tier>(
            new List<Tier>(),
            0,
            1,
            10);

        _repoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.Is<GetAllTiersQuery>(q => q.Name == "NonExistent"),
                            1,
                            10,
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(pagedResult);

        // Act
        var response = await _client.GetAsync("/api/tiers?name=NonExistent");
        var dto = await response.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().BeEmpty();
        dto.TotalCount.Should().Be(0);
        dto.TotalPages.Should().Be(0);

        _repoMock.Verify(r => r.GetPagedByCriteriaAsync(
                                It.Is<GetAllTiersQuery>(q => q.Name == "NonExistent"),
                                1,
                                10,
                                It.IsAny<CancellationToken>()),
                         Times.Once);
    }
}