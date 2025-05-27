using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BuildingBlocks.Core.Pagination;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using wfc.referential.Application.Interfaces;
using wfc.referential.Application.SupportAccounts.Dtos;
using wfc.referential.Domain.PartnerAggregate;
using wfc.referential.Domain.SupportAccountAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.SupportAccountsTests.GetAllTests;

public class GetAllSupportAccountsEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<ISupportAccountRepository> _repoMock = new();

    public GetAllSupportAccountsEndpointTests(WebApplicationFactory<Program> factory)
    {
        var customisedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<ISupportAccountRepository>();
                services.AddSingleton(_repoMock.Object);
            });
        });

        _client = customisedFactory.CreateClient();
    }

    private static SupportAccount CreateTestSupportAccount(string code, string description, decimal threshold, decimal limit, string accountingNumber)
    {
        var partnerId = Guid.NewGuid();

        return SupportAccount.Create(
            SupportAccountId.Of(Guid.NewGuid()),
            code,
            description,
            threshold,
            limit,
            5000.00m,
            accountingNumber
        );
    }

    [Fact(DisplayName = "GET /api/support-accounts returns paged list")]
    public async Task Get_ShouldReturnPagedList_WhenParamsAreValid()
    {
        // Arrange
        var allAccounts = new[] {
            CreateTestSupportAccount("SA001", "Support Account 1", 10000.00m, 20000.00m, "ACC001"),
            CreateTestSupportAccount("SA002", "Support Account 2", 15000.00m, 25000.00m, "ACC002"),
            CreateTestSupportAccount("SA003", "Support Account 3", 20000.00m, 30000.00m, "ACC003"),
            CreateTestSupportAccount("SA004", "Support Account 4", 25000.00m, 35000.00m, "ACC004"),
            CreateTestSupportAccount("SA005", "Support Account 5", 30000.00m, 40000.00m, "ACC005")
        };

        var pagedResult = new PagedResult<SupportAccount>(allAccounts.Take(2).ToList(), allAccounts.Length, 1, 2);

        _repoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.IsAny<object>(),
                            It.IsAny<int>(),
                            It.IsAny<int>(),
                            It.IsAny<CancellationToken>(),
                            It.IsAny<System.Linq.Expressions.Expression<System.Func<SupportAccount, object>>[]>()))
                 .ReturnsAsync(pagedResult);

        // Act
        var response = await _client.GetAsync("/api/support-accounts?pageNumber=1&pageSize=2");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<GetSupportAccountsResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(5);
        result.TotalPages.Should().Be(3);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(2);

        _repoMock.Verify(r => r.GetPagedByCriteriaAsync(
                                It.IsAny<object>(),
                                It.IsAny<int>(),
                                It.IsAny<int>(),
                                It.IsAny<CancellationToken>(),
                                It.IsAny<System.Linq.Expressions.Expression<System.Func<SupportAccount, object>>[]>()),
                         Times.Once);
    }

    [Fact(DisplayName = "GET /api/support-accounts?code=SA001 returns only matching account")]
    public async Task Get_ShouldFilterByCode()
    {
        // Arrange
        var account = CreateTestSupportAccount("SA001", "Support Account 1", 10000.00m, 20000.00m, "ACC001");
        var pagedResult = new PagedResult<SupportAccount>(new List<SupportAccount> { account }, 1, 1, 10);

        _repoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.IsAny<object>(),
                            It.IsAny<int>(),
                            It.IsAny<int>(),
                            It.IsAny<CancellationToken>(),
                            It.IsAny<System.Linq.Expressions.Expression<System.Func<SupportAccount, object>>[]>()))
                 .ReturnsAsync(pagedResult);

        // Act
        var response = await _client.GetAsync("/api/support-accounts?code=SA001");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<GetSupportAccountsResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.Items.Should().HaveCount(1);
        result.Items.First().Code.Should().Be("SA001");

        _repoMock.Verify(r => r.GetPagedByCriteriaAsync(
                                It.IsAny<object>(),
                                It.IsAny<int>(),
                                It.IsAny<int>(),
                                It.IsAny<CancellationToken>(),
                                It.IsAny<System.Linq.Expressions.Expression<System.Func<SupportAccount, object>>[]>()),
                         Times.Once);
    }

    [Fact(DisplayName = "GET /api/support-accounts?isEnabled=false returns only disabled accounts")]
    public async Task Get_ShouldFilterByEnabledStatus()
    {
        // Arrange
        var disabledAccount = CreateTestSupportAccount("SA001", "Disabled Account", 10000.00m, 20000.00m, "ACC001");
        disabledAccount.Disable(); // Make it disabled

        var pagedResult = new PagedResult<SupportAccount>(new List<SupportAccount> { disabledAccount }, 1, 1, 10);

        _repoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.IsAny<object>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(pagedResult);

        // Act
        var response = await _client.GetAsync("/api/support-accounts?isEnabled=false");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<GetSupportAccountsResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.Items.Should().HaveCount(1);
        result.Items.First().IsEnabled.Should().BeFalse();

        _repoMock.Verify(r => r.GetPagedByCriteriaAsync(
                                It.IsAny<object>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
                         Times.Once);
    }

    [Fact(DisplayName = "GET /api/support-accounts returns empty result when no matches found")]
    public async Task Get_ShouldReturnEmptyResult_WhenNoMatchesFound()
    {
        // Arrange
        var emptyResult = new PagedResult<SupportAccount>(new List<SupportAccount>(), 0, 1, 10);

        _repoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.IsAny<object>(),
                            It.IsAny<int>(),
                            It.IsAny<int>(),
                            It.IsAny<CancellationToken>(),
                            It.IsAny<System.Linq.Expressions.Expression<System.Func<SupportAccount, object>>[]>()))
                 .ReturnsAsync(emptyResult);

        // Act
        var response = await _client.GetAsync("/api/support-accounts?code=NONEXISTENT");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<GetSupportAccountsResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }
}