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
using wfc.referential.Application.SupportAccounts.Queries.GetAllSupportAccounts;
using wfc.referential.Domain.PartnerAggregate;
using wfc.referential.Domain.SupportAccountAggregate;
using wfc.referential.Domain.CityAggregate;
using wfc.referential.Domain.SectorAggregate;
using Xunit;
using wfc.referential.Domain.RegionAggregate;

namespace wfc.referential.AcceptanceTests.SupportAccountsTests.GetAllTests;

public class GetAllSupportAccountsEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<ISupportAccountRepository> _repoMock = new();

    public GetAllSupportAccountsEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var customisedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<ISupportAccountRepository>();
                services.RemoveAll<ICacheService>();

                services.AddSingleton(_repoMock.Object);
                services.AddSingleton(cacheMock.Object);
            });
        });

        _client = customisedFactory.CreateClient();
    }

    // Helper to build dummy support accounts quickly
    private static SupportAccount CreateTestSupportAccount(string code, string name, decimal threshold, decimal limit, string accountingNumber, SupportAccountType supportAccountType)
    {
        var partnerId = Guid.NewGuid();


        var cityId = Guid.NewGuid();
        var city = City.Create(CityId.Of(cityId), "C001", "Test City", "timezone", "taxzone", new RegionId(Guid.NewGuid()), null);


        var sectorId = Guid.NewGuid();
        var sector = Sector.Create(SectorId.Of(sectorId), "S001", "Test Sector", city);

        var partner = Partner.Create(
            PartnerId.Of(partnerId),
            "P" + code,
            "Partner for " + name,
            NetworkMode.Franchise,
            PaymentMode.PrePaye,
            "ID" + code,
            supportAccountType,
            "IDNUM" + code,
            "Standard",
            "AUX" + code,
            "ICE" + code,
            "/logos/logo.png",
            sector,
            city
        );

        return SupportAccount.Create(
            SupportAccountId.Of(Guid.NewGuid()),
            code,
            name,
            threshold,
            limit,
            5000.00m,
            accountingNumber,
            partner,
            supportAccountType
        );
    }

    // Lightweight DTO for deserialising the endpoint response
    private record PagedResultDto<T>(T[] Items, int PageNumber, int PageSize,
                                     int TotalCount, int TotalPages);

    [Fact(DisplayName = "GET /api/support-accounts returns paged list")]
    public async Task Get_ShouldReturnPagedList_WhenParamsAreValid()
    {
        // Arrange
        var allAccounts = new[] {
            CreateTestSupportAccount("SA001", "Support Account 1", 10000.00m, 20000.00m, "ACC001", SupportAccountType.Commun),
            CreateTestSupportAccount("SA002", "Support Account 2", 15000.00m, 25000.00m, "ACC002", SupportAccountType.Individuel),
            CreateTestSupportAccount("SA003", "Support Account 3", 20000.00m, 30000.00m, "ACC003", SupportAccountType.Commun),
            CreateTestSupportAccount("SA004", "Support Account 4", 25000.00m, 35000.00m, "ACC004", SupportAccountType.Individuel),
            CreateTestSupportAccount("SA005", "Support Account 5", 30000.00m, 40000.00m, "ACC005", SupportAccountType.Commun)
        };

        // Repository returns first 2 items for page=1 size=2
        _repoMock.Setup(r => r.GetFilteredSupportAccountsAsync(
                            It.Is<GetAllSupportAccountsQuery>(q => q.PageNumber == 1 && q.PageSize == 2),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(allAccounts.Take(2).ToList());

        _repoMock.Setup(r => r.GetCountTotalAsync(
                            It.IsAny<GetAllSupportAccountsQuery>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(allAccounts.Length);

        // Act
        var response = await _client.GetAsync("/api/support-accounts?pageNumber=1&pageSize=2");
        var dto = await response.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(2);
        dto.TotalCount.Should().Be(5);
        dto.TotalPages.Should().Be(3);
        dto.PageNumber.Should().Be(1);
        dto.PageSize.Should().Be(2);

        _repoMock.Verify(r => r.GetFilteredSupportAccountsAsync(
                                It.Is<GetAllSupportAccountsQuery>(q => q.PageNumber == 1 && q.PageSize == 2),
                                It.IsAny<CancellationToken>()),
                         Times.Once);
    }

    [Fact(DisplayName = "GET /api/support-accounts?code=SA001 returns only matching account")]
    public async Task Get_ShouldFilterByCode()
    {
        // Arrange
        var account = CreateTestSupportAccount("SA001", "Support Account 1", 10000.00m, 20000.00m, "ACC001", SupportAccountType.Commun);

        _repoMock.Setup(r => r.GetFilteredSupportAccountsAsync(
                            It.Is<GetAllSupportAccountsQuery>(q => q.Code == "SA001"),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new List<SupportAccount> { account });

        _repoMock.Setup(r => r.GetCountTotalAsync(
                            It.IsAny<GetAllSupportAccountsQuery>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(1);

        // Act
        var response = await _client.GetAsync("/api/support-accounts?code=SA001");
        var dto = await response.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(1);
        dto.Items[0].GetProperty("code").GetString().Should().Be("SA001");

        _repoMock.Verify(r => r.GetFilteredSupportAccountsAsync(
                                It.Is<GetAllSupportAccountsQuery>(q => q.Code == "SA001"),
                                It.IsAny<CancellationToken>()),
                         Times.Once);
    }

    [Fact(DisplayName = "GET /api/support-accounts?supportAccountType=Commun filters by support account type")]
    public async Task Get_ShouldFilterBySupportAccountType()
    {
        // Arrange
        var communAccounts = new[] {
            CreateTestSupportAccount("SA001", "Support Account 1", 10000.00m, 20000.00m, "ACC001", SupportAccountType.Commun),
            CreateTestSupportAccount("SA003", "Support Account 3", 20000.00m, 30000.00m, "ACC003", SupportAccountType.Commun),
            CreateTestSupportAccount("SA005", "Support Account 5", 30000.00m, 40000.00m, "ACC005", SupportAccountType.Commun)
        };

        _repoMock.Setup(r => r.GetFilteredSupportAccountsAsync(
                            It.Is<GetAllSupportAccountsQuery>(q => q.SupportAccountType == "Commun"),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(communAccounts.ToList());

        _repoMock.Setup(r => r.GetCountTotalAsync(
                            It.Is<GetAllSupportAccountsQuery>(q => q.SupportAccountType == "Commun"),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(communAccounts.Length);

        // Act
        var response = await _client.GetAsync("/api/support-accounts?supportAccountType=Commun");
        var dto = await response.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(3);
        dto.TotalCount.Should().Be(3);

        foreach (var item in dto.Items)
        {
            item.GetProperty("supportAccountType").GetString().Should().Be("Commun");
        }

        _repoMock.Verify(r => r.GetFilteredSupportAccountsAsync(
                                It.Is<GetAllSupportAccountsQuery>(q => q.SupportAccountType == "Commun"),
                                It.IsAny<CancellationToken>()),
                         Times.Once);
    }

    [Fact(DisplayName = "GET /api/support-accounts?minThreshold=20000 filters by minimum threshold")]
    public async Task Get_ShouldFilterByMinimumThreshold()
    {
        // Arrange
        var highThresholdAccounts = new[] {
            CreateTestSupportAccount("SA003", "Support Account 3", 20000.00m, 30000.00m, "ACC003", SupportAccountType.Commun),
            CreateTestSupportAccount("SA004", "Support Account 4", 25000.00m, 35000.00m, "ACC004", SupportAccountType.Individuel),
            CreateTestSupportAccount("SA005", "Support Account 5", 30000.00m, 40000.00m, "ACC005", SupportAccountType.Commun)
        };

        _repoMock.Setup(r => r.GetFilteredSupportAccountsAsync(
                            It.Is<GetAllSupportAccountsQuery>(q => q.MinThreshold == 20000m),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(highThresholdAccounts.ToList());

        _repoMock.Setup(r => r.GetCountTotalAsync(
                            It.IsAny<GetAllSupportAccountsQuery>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(highThresholdAccounts.Length);

        // Act
        var response = await _client.GetAsync("/api/support-accounts?minThreshold=20000");
        var dto = await response.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(3);

        foreach (var item in dto.Items)
        {
            item.GetProperty("threshold").GetDecimal().Should().BeGreaterThanOrEqualTo(20000.00m);
        }

        _repoMock.Verify(r => r.GetFilteredSupportAccountsAsync(
                                It.Is<GetAllSupportAccountsQuery>(q => q.MinThreshold == 20000m),
                                It.IsAny<CancellationToken>()),
                         Times.Once);
    }

    [Fact(DisplayName = "GET /api/support-accounts?minLimit=30000 filters by minimum limit")]
    public async Task Get_ShouldFilterByMinimumLimit()
    {
        // Arrange
        var highLimitAccounts = new[] {
            CreateTestSupportAccount("SA003", "Support Account 3", 20000.00m, 30000.00m, "ACC003", SupportAccountType.Commun),
            CreateTestSupportAccount("SA004", "Support Account 4", 25000.00m, 35000.00m, "ACC004", SupportAccountType.Individuel),
            CreateTestSupportAccount("SA005", "Support Account 5", 30000.00m, 40000.00m, "ACC005", SupportAccountType.Commun)
        };

        _repoMock.Setup(r => r.GetFilteredSupportAccountsAsync(
                            It.Is<GetAllSupportAccountsQuery>(q => q.MinLimit == 30000m),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(highLimitAccounts.ToList());

        _repoMock.Setup(r => r.GetCountTotalAsync(
                            It.IsAny<GetAllSupportAccountsQuery>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(highLimitAccounts.Length);

        // Act
        var response = await _client.GetAsync("/api/support-accounts?minLimit=30000");
        var dto = await response.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(3);

        foreach (var item in dto.Items)
        {
            item.GetProperty("limit").GetDecimal().Should().BeGreaterThanOrEqualTo(30000.00m);
        }

        _repoMock.Verify(r => r.GetFilteredSupportAccountsAsync(
                                It.Is<GetAllSupportAccountsQuery>(q => q.MinLimit == 30000m),
                                It.IsAny<CancellationToken>()),
                         Times.Once);
    }

    [Fact(DisplayName = "GET /api/support-accounts?isEnabled=false returns only disabled accounts")]
    public async Task Get_ShouldFilterByEnabledStatus()
    {
        // Arrange
        var disabledAccount = CreateTestSupportAccount("SA001", "Disabled Account", 10000.00m, 20000.00m, "ACC001", SupportAccountType.Commun);
        disabledAccount.Disable(); // Make it disabled

        _repoMock.Setup(r => r.GetFilteredSupportAccountsAsync(
                            It.Is<GetAllSupportAccountsQuery>(q => q.IsEnabled == false),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new List<SupportAccount> { disabledAccount });

        _repoMock.Setup(r => r.GetCountTotalAsync(
                            It.IsAny<GetAllSupportAccountsQuery>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(1);

        // Act
        var response = await _client.GetAsync("/api/support-accounts?isEnabled=false");
        var dto = await response.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(1);
        dto.Items[0].GetProperty("isEnabled").GetBoolean().Should().BeFalse();

        _repoMock.Verify(r => r.GetFilteredSupportAccountsAsync(
                                It.Is<GetAllSupportAccountsQuery>(q => q.IsEnabled == false),
                                It.IsAny<CancellationToken>()),
                         Times.Once);
    }
}