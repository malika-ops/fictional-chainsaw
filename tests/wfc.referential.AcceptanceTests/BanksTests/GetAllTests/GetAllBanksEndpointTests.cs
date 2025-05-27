using System.Net;
using System.Net.Http.Json;
using BuildingBlocks.Core.Pagination;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using wfc.referential.Application.Banks.Dtos;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.BankAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.BanksTests.SearchTests;

public class GetAllBanksAcceptanceTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IBankRepository> _repoMock = new();

    public GetAllBanksAcceptanceTests(WebApplicationFactory<Program> factory)
    {
        var customizedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IBankRepository>();
                services.AddSingleton(_repoMock.Object);
            });
        });
        _client = customizedFactory.CreateClient();
    }

    [Fact(DisplayName = "GET /api/banks returns all banks using search criteria")]
    public async Task GetAllBanks_Should_ReturnAllBanks_UsingSearchCriteria()
    {
        // Arrange
        var banks = new List<Bank>
        {
            Bank.Create(BankId.Of(Guid.NewGuid()), "AWB", "Attijariwafa Bank", "AWB"),
            Bank.Create(BankId.Of(Guid.NewGuid()), "BMCE", "Banque Marocaine du Commerce Extérieur", "BMCE"),
            Bank.Create(BankId.Of(Guid.NewGuid()), "SG", "Société Générale Maroc", "SG")
        };

        var pagedResult = new PagedResult<Bank>(banks, banks.Count, 1, 10);

        _repoMock.Setup(r => r.GetPagedByCriteriaAsync(
            It.IsAny<object>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var response = await _client.GetAsync("/api/banks?PageNumber=1&PageSize=10");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<GetBanksResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(3);
        result.TotalCount.Should().Be(3);

        // Verify all bank data is returned
        result.Items.Should().Contain(b => b.Code == "AWB" && b.Name == "Attijariwafa Bank");
        result.Items.Should().Contain(b => b.Code == "BMCE" && b.Name == "Banque Marocaine du Commerce Extérieur");
        result.Items.Should().Contain(b => b.Code == "SG" && b.Name == "Société Générale Maroc");
    }

    [Fact(DisplayName = "GET /api/banks supports filtering by BankCode")]
    public async Task GetAllBanks_Should_FilterByBankCode_WhenCodeProvided()
    {
        // Arrange
        var filteredBanks = new List<Bank>
        {
            Bank.Create(BankId.Of(Guid.NewGuid()), "AWB", "Attijariwafa Bank", "AWB")
        };

        var pagedResult = new PagedResult<Bank>(filteredBanks, 1, 1, 10);

        _repoMock.Setup(r => r.GetPagedByCriteriaAsync(
            It.IsAny<object>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var response = await _client.GetAsync("/api/banks?Code=AWB&PageNumber=1&PageSize=10");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<GetBanksResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items.First().Code.Should().Be("AWB");
    }

    [Fact(DisplayName = "GET /api/banks supports filtering by BankName")]
    public async Task GetAllBanks_Should_FilterByBankName_WhenNameProvided()
    {
        // Arrange
        var filteredBanks = new List<Bank>
        {
            Bank.Create(BankId.Of(Guid.NewGuid()), "AWB", "Attijariwafa Bank", "AWB"),
            Bank.Create(BankId.Of(Guid.NewGuid()), "CBM", "Commercial Bank of Morocco", "CBM")
        };

        var pagedResult = new PagedResult<Bank>(filteredBanks, 2, 1, 10);

        _repoMock.Setup(r => r.GetPagedByCriteriaAsync(
            It.IsAny<object>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var response = await _client.GetAsync("/api/banks?Name=Bank&PageNumber=1&PageSize=10");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<GetBanksResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.Items.Should().AllSatisfy(b => b.Name.Should().Contain("Bank"));
    }

    [Fact(DisplayName = "GET /api/banks supports filtering by IsEnabled status")]
    public async Task GetAllBanks_Should_FilterByIsEnabled_WhenStatusProvided()
    {
        // Arrange
        var activeBanks = new List<Bank>
        {
            Bank.Create(BankId.Of(Guid.NewGuid()), "AWB", "Attijariwafa Bank", "AWB"),
            Bank.Create(BankId.Of(Guid.NewGuid()), "BMCE", "Banque Marocaine du Commerce Extérieur", "BMCE")
        };

        var pagedResult = new PagedResult<Bank>(activeBanks, 2, 1, 10);

        _repoMock.Setup(r => r.GetPagedByCriteriaAsync(
            It.IsAny<object>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var response = await _client.GetAsync("/api/banks?IsEnabled=true&PageNumber=1&PageSize=10");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<GetBanksResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.Items.Should().AllSatisfy(b => b.IsEnabled.Should().BeTrue());
    }

    [Fact(DisplayName = "GET /api/banks returns paginated results")]
    public async Task GetAllBanks_Should_ReturnPaginatedResults_WhenPaginationParametersProvided()
    {
        // Arrange
        var banks = new List<Bank>
        {
            Bank.Create(BankId.Of(Guid.NewGuid()), "AWB", "Attijariwafa Bank", "AWB"),
            Bank.Create(BankId.Of(Guid.NewGuid()), "BMCE", "Banque Marocaine du Commerce Extérieur", "BMCE")
        };

        var pagedResult = new PagedResult<Bank>(banks, 25, 2, 10); // Page 2 of 3 pages (25 total items)

        _repoMock.Setup(r => r.GetPagedByCriteriaAsync(
            It.IsAny<object>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var response = await _client.GetAsync("/api/banks?PageNumber=2&PageSize=10");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<GetBanksResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result.PageNumber.Should().Be(2);
        result.PageSize.Should().Be(10);
        result.TotalCount.Should().Be(25);
        result.Items.Should().HaveCount(2);
    }

    [Fact(DisplayName = "GET /api/banks returns empty result when no matches found")]
    public async Task GetAllBanks_Should_ReturnEmptyResult_WhenNoMatchesFound()
    {
        // Arrange
        var emptyResult = new PagedResult<Bank>(new List<Bank>(), 0, 1, 10);

        _repoMock.Setup(r => r.GetPagedByCriteriaAsync(
            It.IsAny<object>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyResult);

        // Act
        var response = await _client.GetAsync("/api/banks?Code=NONEXISTENT");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<GetBanksResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact(DisplayName = "GET /api/banks supports multiple search criteria simultaneously")]
    public async Task GetAllBanks_Should_SupportMultipleCriteria_WhenMultipleFiltersProvided()
    {
        // Arrange
        var filteredBanks = new List<Bank>
        {
            Bank.Create(BankId.Of(Guid.NewGuid()), "AWB", "Attijariwafa Bank", "AWB")
        };

        var pagedResult = new PagedResult<Bank>(filteredBanks, 1, 1, 10);

        _repoMock.Setup(r => r.GetPagedByCriteriaAsync(
            It.IsAny<object>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act - Search with multiple criteria
        var response = await _client.GetAsync("/api/banks?Code=AWB&Name=Attijariwafa&IsEnabled=true");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<GetBanksResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);

        var bank = result.Items.First();
        bank.Code.Should().Be("AWB");
        bank.Name.Should().Be("Attijariwafa Bank");
        bank.IsEnabled.Should().BeTrue();
    }

    [Theory(DisplayName = "GET /api/banks supports various search criteria")]
    [InlineData("Code", "AWB")]
    [InlineData("Name", "Bank")]
    [InlineData("Abbreviation", "AWB")]
    public async Task GetAllBanks_Should_SupportVariousSearchCriteria(string filterType, string filterValue)
    {
        // Arrange
        var banks = new List<Bank>
        {
            Bank.Create(BankId.Of(Guid.NewGuid()), "AWB", "Attijariwafa Bank", "AWB")
        };

        var pagedResult = new PagedResult<Bank>(banks, 1, 1, 10);

        _repoMock.Setup(r => r.GetPagedByCriteriaAsync(
            It.IsAny<object>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var response = await _client.GetAsync($"/api/banks?{filterType}={filterValue}");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<GetBanksResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);

        _repoMock.Verify(r => r.GetPagedByCriteriaAsync(
            It.IsAny<object>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "GET /api/banks returns all bank data in response")]
    public async Task GetAllBanks_Should_ReturnAllBankData_InResponse()
    {
        // Arrange
        var banks = new List<Bank>
        {
            Bank.Create(BankId.Of(Guid.NewGuid()), "AWB", "Attijariwafa Bank", "AWB")
        };

        var pagedResult = new PagedResult<Bank>(banks, 1, 1, 10);

        _repoMock.Setup(r => r.GetPagedByCriteriaAsync(
            It.IsAny<object>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var response = await _client.GetAsync("/api/banks");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<GetBanksResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);

        var bank = result.Items.First();
        bank.BankId.Should().NotBeEmpty();
        bank.Code.Should().Be("AWB");
        bank.Name.Should().Be("Attijariwafa Bank");
        bank.Abbreviation.Should().Be("AWB");
        bank.IsEnabled.Should().BeTrue();
    }
}
