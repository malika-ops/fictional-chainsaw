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
using wfc.referential.Application.PartnerAccounts.Queries.GetAllPartnerAccounts;
using wfc.referential.Domain.BankAggregate;
using wfc.referential.Domain.PartnerAccountAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.PartnerAccountsTests.GetAllTests;

public class GetAllPartnerAccountsEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IPartnerAccountRepository> _repoMock = new();

    public GetAllPartnerAccountsEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var customisedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IPartnerAccountRepository>();
                services.RemoveAll<ICacheService>();

                services.AddSingleton(_repoMock.Object);
                services.AddSingleton(cacheMock.Object);
            });
        });

        _client = customisedFactory.CreateClient();
    }

    // Helper to build dummy partner accounts quickly
    private static PartnerAccount CreateTestPartnerAccount(string accountNumber, string rib, string businessName, AccountType accountType)
    {
        var bankId = Guid.NewGuid();
        var bank = Bank.Create(BankId.Of(bankId), "AWB", "Attijariwafa Bank", "AWB");

        return PartnerAccount.Create(
            PartnerAccountId.Of(Guid.NewGuid()),
            accountNumber,
            rib,
            "Casablanca Centre",
            businessName,
            businessName.Substring(0, 2).ToUpper(),
            50000.00m,
            bank,
            accountType
        );
    }

    // Lightweight DTO for deserialising the endpoint response
    private record PagedResultDto<T>(T[] Items, int PageNumber, int PageSize,
                                     int TotalCount, int TotalPages);

    [Fact(DisplayName = "GET /api/partner-accounts returns paged list")]
    public async Task Get_ShouldReturnPagedList_WhenParamsAreValid()
    {
        // Arrange
        var allAccounts = new[] {
            CreateTestPartnerAccount("000123456789", "12345678901234567890123", "Wafa Cash Services", AccountType.Activité),
            CreateTestPartnerAccount("000987654321", "98765432109876543210987", "Transfert Express", AccountType.Commission),
            CreateTestPartnerAccount("000111222333", "11122233344455566677788", "Rapid Transfer", AccountType.Activité),
            CreateTestPartnerAccount("000444555666", "44455566677788899900011", "Money Express", AccountType.Commission),
            CreateTestPartnerAccount("000777888999", "77788899900011122233344", "Quick Cash", AccountType.Activité)
        };

        // Repository returns first 2 items for page=1 size=2
        _repoMock.Setup(r => r.GetFilteredPartnerAccountsAsync(
                            It.Is<GetAllPartnerAccountsQuery>(q => q.PageNumber == 1 && q.PageSize == 2),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(allAccounts.Take(2).ToList());

        _repoMock.Setup(r => r.GetCountTotalAsync(
                            It.IsAny<GetAllPartnerAccountsQuery>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(allAccounts.Length);

        // Act
        var response = await _client.GetAsync("/api/partner-accounts?pageNumber=1&pageSize=2");
        var dto = await response.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(2);
        dto.TotalCount.Should().Be(5);
        dto.TotalPages.Should().Be(3);
        dto.PageNumber.Should().Be(1);
        dto.PageSize.Should().Be(2);

        _repoMock.Verify(r => r.GetFilteredPartnerAccountsAsync(
                                It.Is<GetAllPartnerAccountsQuery>(q => q.PageNumber == 1 && q.PageSize == 2),
                                It.IsAny<CancellationToken>()),
                         Times.Once);
    }

    [Fact(DisplayName = "GET /api/partner-accounts?accountNumber=000123456789 returns only matching account")]
    public async Task Get_ShouldFilterByAccountNumber()
    {
        // Arrange
        var account = CreateTestPartnerAccount("000123456789", "12345678901234567890123", "Wafa Cash Services", AccountType.Activité);

        _repoMock.Setup(r => r.GetFilteredPartnerAccountsAsync(
                            It.Is<GetAllPartnerAccountsQuery>(q => q.AccountNumber == "000123456789"),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new List<PartnerAccount> { account });

        _repoMock.Setup(r => r.GetCountTotalAsync(
                            It.IsAny<GetAllPartnerAccountsQuery>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(1);

        // Act
        var response = await _client.GetAsync("/api/partner-accounts?accountNumber=000123456789");
        var dto = await response.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(1);
        dto.Items[0].GetProperty("accountNumber").GetString().Should().Be("000123456789");

        _repoMock.Verify(r => r.GetFilteredPartnerAccountsAsync(
                                It.Is<GetAllPartnerAccountsQuery>(q => q.AccountNumber == "000123456789"),
                                It.IsAny<CancellationToken>()),
                         Times.Once);
    }

    [Fact(DisplayName = "GET /api/partner-accounts?accountType=Activité filters by account type")]
    public async Task Get_ShouldFilterByAccountType()
    {
        // Arrange
        var activiteAccounts = new[] {
            CreateTestPartnerAccount("000123456789", "12345678901234567890123", "Wafa Cash Services", AccountType.Activité),
            CreateTestPartnerAccount("000111222333", "11122233344455566677788", "Rapid Transfer", AccountType.Activité),
            CreateTestPartnerAccount("000777888999", "77788899900011122233344", "Quick Cash", AccountType.Activité)
        };

        _repoMock.Setup(r => r.GetFilteredPartnerAccountsAsync(
                            It.Is<GetAllPartnerAccountsQuery>(q => q.AccountType == "Activité"),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(activiteAccounts.ToList());

        _repoMock.Setup(r => r.GetCountTotalAsync(
                            It.Is<GetAllPartnerAccountsQuery>(q => q.AccountType == "Activité"),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(activiteAccounts.Length);

        // Act
        var response = await _client.GetAsync("/api/partner-accounts?accountType=Activité");
        var dto = await response.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(3);
        dto.TotalCount.Should().Be(3);

        foreach (var item in dto.Items)
        {
            item.GetProperty("accountType").GetString().Should().Be("Activité");
        }

        _repoMock.Verify(r => r.GetFilteredPartnerAccountsAsync(
                                It.Is<GetAllPartnerAccountsQuery>(q => q.AccountType == "Activité"),
                                It.IsAny<CancellationToken>()),
                         Times.Once);
    }

    [Fact(DisplayName = "GET /api/partner-accounts?minAccountBalance=60000 filters by minimum balance")]
    public async Task Get_ShouldFilterByMinimumBalance()
    {
        // Arrange
        var highBalanceAccount = CreateTestPartnerAccount("000123456789", "12345678901234567890123", "High Balance Account", AccountType.Activité);

        // Assume the internal balance is set higher than the original 50000
        var partnerAccountType = typeof(PartnerAccount);
        var balanceProperty = partnerAccountType.GetProperty("AccountBalance");
        balanceProperty!.SetValue(highBalanceAccount, 75000.00m);

        _repoMock.Setup(r => r.GetFilteredPartnerAccountsAsync(
                            It.Is<GetAllPartnerAccountsQuery>(q => q.MinAccountBalance == 60000m),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new List<PartnerAccount> { highBalanceAccount });

        _repoMock.Setup(r => r.GetCountTotalAsync(
                            It.IsAny<GetAllPartnerAccountsQuery>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(1);

        // Act
        var response = await _client.GetAsync("/api/partner-accounts?minAccountBalance=60000");
        var dto = await response.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(1);
        dto.Items[0].GetProperty("accountBalance").GetDecimal().Should().Be(75000.00m);

        _repoMock.Verify(r => r.GetFilteredPartnerAccountsAsync(
                                It.Is<GetAllPartnerAccountsQuery>(q => q.MinAccountBalance == 60000m),
                                It.IsAny<CancellationToken>()),
                         Times.Once);
    }

    [Fact(DisplayName = "GET /api/partner-accounts?isEnabled=false returns only disabled accounts")]
    public async Task Get_ShouldFilterByEnabledStatus()
    {
        // Arrange
        var disabledAccount = CreateTestPartnerAccount("000123456789", "12345678901234567890123", "Disabled Account", AccountType.Activité);
        disabledAccount.Disable(); // Make it disabled

        _repoMock.Setup(r => r.GetFilteredPartnerAccountsAsync(
                            It.Is<GetAllPartnerAccountsQuery>(q => q.IsEnabled == false),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new List<PartnerAccount> { disabledAccount });

        _repoMock.Setup(r => r.GetCountTotalAsync(
                            It.IsAny<GetAllPartnerAccountsQuery>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(1);

        // Act
        var response = await _client.GetAsync("/api/partner-accounts?isEnabled=false");
        var dto = await response.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(1);
        dto.Items[0].GetProperty("isEnabled").GetBoolean().Should().BeFalse();

        _repoMock.Verify(r => r.GetFilteredPartnerAccountsAsync(
                                It.Is<GetAllPartnerAccountsQuery>(q => q.IsEnabled == false),
                                It.IsAny<CancellationToken>()),
                         Times.Once);
    }
}