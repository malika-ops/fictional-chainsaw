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
using wfc.referential.Application.Partners.Queries.GetAllPartners;
using wfc.referential.Domain.CityAggregate;
using wfc.referential.Domain.PartnerAggregate;
using wfc.referential.Domain.RegionAggregate;
using wfc.referential.Domain.SectorAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.PartnersTests.GetAllTests;

public class GetAllPartnersEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IPartnerRepository> _repoMock = new();

    public GetAllPartnersEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var customisedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IPartnerRepository>();
                services.RemoveAll<ICacheService>();

                services.AddSingleton(_repoMock.Object);
                services.AddSingleton(cacheMock.Object);
            });
        });

        _client = customisedFactory.CreateClient();
    }

    // Helper to build dummy partners quickly
    private static Partner CreateTestPartner(string code, string label, NetworkMode networkMode, string identificationNumber)
    {
        
        var cityId = Guid.NewGuid();
        var city = City.Create(CityId.Of(cityId), "C001", "Test City", "timezone", "taxzone", new RegionId(Guid.NewGuid()), null);


        var sectorId = Guid.NewGuid();
        var sector = Sector.Create(SectorId.Of(sectorId), "S001", "Test Sector", city);

        return Partner.Create(
            PartnerId.Of(Guid.NewGuid()),
            code,
            label,
            networkMode,
            PaymentMode.PrePaye,
            "ID" + code,
            Domain.SupportAccountAggregate.SupportAccountType.Commun,
            identificationNumber,
            "Standard",
            "AUX" + code,
            "ICE" + code,
            "/logos/logo.png",
            sector,
            city
        );
    }

    // Lightweight DTO for deserialising the endpoint response
    private record PagedResultDto<T>(T[] Items, int PageNumber, int PageSize,
                                     int TotalCount, int TotalPages);

    [Fact(DisplayName = "GET /api/partners returns paged list")]
    public async Task Get_ShouldReturnPagedList_WhenParamsAreValid()
    {
        // Arrange
        var allPartners = new[] {
            CreateTestPartner("PTN001", "Partner 1", NetworkMode.Franchise, "ID1"),
            CreateTestPartner("PTN002", "Partner 2", NetworkMode.Succursale, "ID2"),
            CreateTestPartner("PTN003", "Partner 3", NetworkMode.VRP, "ID3"),
            CreateTestPartner("PTN004", "Partner 4", NetworkMode.Prestataire, "ID4"),
            CreateTestPartner("PTN005", "Partner 5", NetworkMode.Franchise, "ID5")
        };

        // Repository returns first 2 items for page=1 size=2
        _repoMock.Setup(r => r.GetFilteredPartnersAsync(
                            It.Is<GetAllPartnersQuery>(q => q.PageNumber == 1 && q.PageSize == 2),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(allPartners.Take(2).ToList());

        _repoMock.Setup(r => r.GetCountTotalAsync(
                            It.IsAny<GetAllPartnersQuery>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(allPartners.Length);

        // Act
        var response = await _client.GetAsync("/api/partners?pageNumber=1&pageSize=2");
        var dto = await response.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(2);
        dto.TotalCount.Should().Be(5);
        dto.TotalPages.Should().Be(3);
        dto.PageNumber.Should().Be(1);
        dto.PageSize.Should().Be(2);

        _repoMock.Verify(r => r.GetFilteredPartnersAsync(
                                It.Is<GetAllPartnersQuery>(q => q.PageNumber == 1 && q.PageSize == 2),
                                It.IsAny<CancellationToken>()),
                         Times.Once);
    }

    [Fact(DisplayName = "GET /api/partners?code=PTN001 returns only matching partner")]
    public async Task Get_ShouldFilterByCode()
    {
        // Arrange
        var partner = CreateTestPartner("PTN001", "Partner 1", NetworkMode.Franchise, "ID1");

        _repoMock.Setup(r => r.GetFilteredPartnersAsync(
                            It.Is<GetAllPartnersQuery>(q => q.Code == "PTN001"),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new List<Partner> { partner });

        _repoMock.Setup(r => r.GetCountTotalAsync(
                            It.IsAny<GetAllPartnersQuery>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(1);

        // Act
        var response = await _client.GetAsync("/api/partners?code=PTN001");
        var dto = await response.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(1);
        dto.Items[0].GetProperty("code").GetString().Should().Be("PTN001");

        _repoMock.Verify(r => r.GetFilteredPartnersAsync(
                                It.Is<GetAllPartnersQuery>(q => q.Code == "PTN001"),
                                It.IsAny<CancellationToken>()),
                         Times.Once);
    }

    [Fact(DisplayName = "GET /api/partners?networkMode=Franchise filters by network mode")]
    public async Task Get_ShouldFilterByNetworkMode()
    {
        // Arrange
        var franchisePartners = new[] {
            CreateTestPartner("PTN001", "Partner 1", NetworkMode.Franchise, "ID1"),
            CreateTestPartner("PTN005", "Partner 5", NetworkMode.Franchise, "ID5")
        };

        _repoMock.Setup(r => r.GetFilteredPartnersAsync(
                            It.Is<GetAllPartnersQuery>(q => q.NetworkMode == "Franchise"),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(franchisePartners.ToList());

        _repoMock.Setup(r => r.GetCountTotalAsync(
                            It.Is<GetAllPartnersQuery>(q => q.NetworkMode == "Franchise"),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(franchisePartners.Length);

        // Act
        var response = await _client.GetAsync("/api/partners?networkMode=Franchise");
        var dto = await response.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(2);
        dto.TotalCount.Should().Be(2);

        foreach (var item in dto.Items)
        {
            item.GetProperty("networkMode").GetString().Should().Be("Franchise");
        }

        _repoMock.Verify(r => r.GetFilteredPartnersAsync(
                                It.Is<GetAllPartnersQuery>(q => q.NetworkMode == "Franchise"),
                                It.IsAny<CancellationToken>()),
                         Times.Once);
    }

    [Fact(DisplayName = "GET /api/partners?isEnabled=false returns only disabled partners")]
    public async Task Get_ShouldFilterByEnabledStatus()
    {
        // Arrange
        var disabledPartner = CreateTestPartner("PTN001", "Disabled Partner", NetworkMode.Franchise, "ID1");
        disabledPartner.Disable(); // Make it disabled

        _repoMock.Setup(r => r.GetFilteredPartnersAsync(
                            It.Is<GetAllPartnersQuery>(q => q.IsEnabled == false),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new List<Partner> { disabledPartner });

        _repoMock.Setup(r => r.GetCountTotalAsync(
                            It.IsAny<GetAllPartnersQuery>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(1);

        // Act
        var response = await _client.GetAsync("/api/partners?isEnabled=false");
        var dto = await response.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(1);
        dto.Items[0].GetProperty("isEnabled").GetBoolean().Should().BeFalse();

        _repoMock.Verify(r => r.GetFilteredPartnersAsync(
                                It.Is<GetAllPartnersQuery>(q => q.IsEnabled == false),
                                It.IsAny<CancellationToken>()),
                         Times.Once);
    }
}