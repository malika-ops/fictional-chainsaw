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
using wfc.referential.Application.Banks.Queries.GetAllBanks;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.BankAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.BanksTests.GetAllTests;

public class GetAllBanksEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IBankRepository> _repoMock = new();

    public GetAllBanksEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var customisedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IBankRepository>();
                services.RemoveAll<ICacheService>();

                services.AddSingleton(_repoMock.Object);
                services.AddSingleton(cacheMock.Object);
            });
        });

        _client = customisedFactory.CreateClient();
    }

    // Helper to build dummy banks quickly
    private static Bank CreateTestBank(string code, string name, string abbreviation)
    {
        return Bank.Create(BankId.Of(Guid.NewGuid()), code, name, abbreviation);
    }

    // Lightweight DTO for deserialising the endpoint response
    private record PagedResultDto<T>(T[] Items, int PageNumber, int PageSize,
                                     int TotalCount, int TotalPages);

    [Fact(DisplayName = "GET /api/banks returns paged list")]
    public async Task Get_ShouldReturnPagedList_WhenParamsAreValid()
    {
        // Arrange
        var allBanks = new[] {
            CreateTestBank("AWB", "Attijariwafa Bank", "AWB"),
            CreateTestBank("BMCE", "Banque Marocaine du Commerce Extérieur", "BMCE"),
            CreateTestBank("SG", "Société Générale Maroc", "SG"),
            CreateTestBank("BP", "Banque Populaire", "BP"),
            CreateTestBank("BMCI", "Banque Marocaine pour le Commerce et l'Industrie", "BMCI")
        };

        // Repository returns first 2 items for page=1 size=2
        _repoMock.Setup(r => r.GetFilteredBanksAsync(
                            It.Is<GetAllBanksQuery>(q => q.PageNumber == 1 && q.PageSize == 2),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(allBanks.Take(2).ToList());

        _repoMock.Setup(r => r.GetCountTotalAsync(
                            It.IsAny<GetAllBanksQuery>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(allBanks.Length);

        // Act
        var response = await _client.GetAsync("/api/banks?pageNumber=1&pageSize=2");
        var dto = await response.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(2);
        dto.TotalCount.Should().Be(5);
        dto.TotalPages.Should().Be(3);
        dto.PageNumber.Should().Be(1);
        dto.PageSize.Should().Be(2);

        _repoMock.Verify(r => r.GetFilteredBanksAsync(
                                It.Is<GetAllBanksQuery>(q => q.PageNumber == 1 && q.PageSize == 2),
                                It.IsAny<CancellationToken>()),
                         Times.Once);
    }

    [Fact(DisplayName = "GET /api/banks?code=AWB returns only matching bank")]
    public async Task Get_ShouldFilterByCode()
    {
        // Arrange
        var bank = CreateTestBank("AWB", "Attijariwafa Bank", "AWB");

        _repoMock.Setup(r => r.GetFilteredBanksAsync(
                            It.Is<GetAllBanksQuery>(q => q.Code == "AWB"),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new List<Bank> { bank });

        _repoMock.Setup(r => r.GetCountTotalAsync(
                            It.IsAny<GetAllBanksQuery>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(1);

        // Act
        var response = await _client.GetAsync("/api/banks?code=AWB");
        var dto = await response.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(1);
        dto.Items[0].GetProperty("code").GetString().Should().Be("AWB");

        _repoMock.Verify(r => r.GetFilteredBanksAsync(
                                It.Is<GetAllBanksQuery>(q => q.Code == "AWB"),
                                It.IsAny<CancellationToken>()),
                         Times.Once);
    }

    [Fact(DisplayName = "GET /api/banks?isEnabled=false returns only disabled banks")]
    public async Task Get_ShouldFilterByEnabledStatus()
    {
        // Arrange
        var bank = CreateTestBank("AWB", "Attijariwafa Bank", "AWB");
        bank.Disable(); // Make it disabled

        _repoMock.Setup(r => r.GetFilteredBanksAsync(
                            It.Is<GetAllBanksQuery>(q => q.IsEnabled == false),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new List<Bank> { bank });

        _repoMock.Setup(r => r.GetCountTotalAsync(
                            It.IsAny<GetAllBanksQuery>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(1);

        // Act
        var response = await _client.GetAsync("/api/banks?isEnabled=false");
        var dto = await response.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(1);
        dto.Items[0].GetProperty("isEnabled").GetBoolean().Should().BeFalse();

        _repoMock.Verify(r => r.GetFilteredBanksAsync(
                                It.Is<GetAllBanksQuery>(q => q.IsEnabled == false),
                                It.IsAny<CancellationToken>()),
                         Times.Once);
    }

    [Fact(DisplayName = "GET /api/banks uses default paging when no query params supplied")]
    public async Task Get_ShouldUseDefaultPaging_WhenNoParamsProvided()
    {
        // Arrange
        // We'll return 3 items – fewer than the default pageSize (10)
        var banks = new[] {
            CreateTestBank("AWB", "Attijariwafa Bank", "AWB"),
            CreateTestBank("BMCE", "Banque Marocaine du Commerce Extérieur", "BMCE"),
            CreateTestBank("SG", "Société Générale Maroc", "SG")
        };

        _repoMock.Setup(r => r.GetFilteredBanksAsync(
                            It.Is<GetAllBanksQuery>(q => q.PageNumber == 1 && q.PageSize == 10),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(banks.ToList());

        _repoMock.Setup(r => r.GetCountTotalAsync(
                            It.IsAny<GetAllBanksQuery>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(banks.Length);

        // Act
        var response = await _client.GetAsync("/api/banks");
        var dto = await response.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        dto!.PageNumber.Should().Be(1);
        dto.PageSize.Should().Be(10);
        dto.Items.Should().HaveCount(3);

        // Repository must have been called with default paging values
        _repoMock.Verify(r => r.GetFilteredBanksAsync(
                                It.Is<GetAllBanksQuery>(q => q.PageNumber == 1 && q.PageSize == 10),
                                It.IsAny<CancellationToken>()),
                         Times.Once);
    }
}