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
using wfc.referential.Domain.BankAggregate;
using wfc.referential.Domain.PartnerAccountAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.PartnerAccountsTests.UpdateBalanceTests;

public class UpdateBalanceEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IPartnerAccountRepository> _repoMock = new();

    public UpdateBalanceEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var customisedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IPartnerAccountRepository>();
                services.RemoveAll<ICacheService>();

                // Default noop for Update
                _repoMock
                    .Setup(r => r.UpdatePartnerAccountAsync(It.IsAny<PartnerAccount>(),
                                                           It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

                services.AddSingleton(_repoMock.Object);
                services.AddSingleton(cacheMock.Object);
            });
        });

        _client = customisedFactory.CreateClient();
    }

    // Helper to create a test partner account
    private static PartnerAccount CreateTestPartnerAccount(Guid id, string accountNumber, string rib, decimal balance)
    {
        var bankId = Guid.NewGuid();
        var bank = Bank.Create(BankId.Of(bankId), "AWB", "Attijariwafa Bank", "AWB");

        return PartnerAccount.Create(
            new PartnerAccountId(id),
            accountNumber,
            rib,
            "Casablanca Centre",
            "Test Business",
            "TB",
            balance,
            bank,
            AccountType.Activité
        );
    }

    [Fact(DisplayName = "PATCH /api/partner-accounts/{id}/balance returns 200 and updates only the balance")]
    public async Task Patch_ShouldReturn200_AndUpdateOnlyBalance()
    {
        // Arrange
        var id = Guid.NewGuid();
        var partnerAccount = CreateTestPartnerAccount(id, "000123456789", "12345678901234567890123", 50000.00m);

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<PartnerAccountId>(pid => pid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(partnerAccount);

        PartnerAccount? updated = null;
        _repoMock.Setup(r => r.UpdatePartnerAccountAsync(It.IsAny<PartnerAccount>(),
                                                        It.IsAny<CancellationToken>()))
                 .Callback<PartnerAccount, CancellationToken>((p, _) => updated = p)
                 .Returns(Task.CompletedTask);

        var payload = new
        {
            PartnerAccountId = id,
            NewBalance = 75000.00m
        };

        // Act
        var response = await _client.PatchAsync($"/api/partner-accounts/{id}/balance", JsonContent.Create(payload));
        var returned = await response.Content.ReadFromJsonAsync<Guid>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returned.Should().Be(id);

        updated!.AccountBalance.Should().Be(75000.00m);  // Balance should change
        updated.AccountNumber.Should().Be("000123456789"); // Other fields should not change
        updated.RIB.Should().Be("12345678901234567890123");

        _repoMock.Verify(r => r.UpdatePartnerAccountAsync(It.IsAny<PartnerAccount>(),
                                                          It.IsAny<CancellationToken>()),
                          Times.Once);
    }

    [Fact(DisplayName = "PATCH /api/partner-accounts/{id}/balance returns 400 when account doesn't exist")]
    public async Task Patch_ShouldReturn400_WhenAccountDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<PartnerAccountId>(pid => pid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((PartnerAccount?)null);

        var payload = new
        {
            PartnerAccountId = id,
            NewBalance = 75000.00m
        };

        // Act
        var response = await _client.PatchAsync($"/api/partner-accounts/{id}/balance", JsonContent.Create(payload));
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors").GetString()
           .Should().Be($"Partner account with ID {id} not found");

        _repoMock.Verify(r => r.UpdatePartnerAccountAsync(It.IsAny<PartnerAccount>(),
                                                         It.IsAny<CancellationToken>()),
                         Times.Never);
    }

    [Fact(DisplayName = "PATCH /api/partner-accounts/{id}/balance returns 400 when balance is negative")]
    public async Task Patch_ShouldReturn400_WhenBalanceIsNegative()
    {
        // Arrange
        var id = Guid.NewGuid();
        var partnerAccount = CreateTestPartnerAccount(id, "000123456789", "12345678901234567890123", 50000.00m);

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<PartnerAccountId>(pid => pid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(partnerAccount);

        var payload = new
        {
            PartnerAccountId = id,
            NewBalance = -1000.00m // Negative balance
        };

        // Act
        var response = await _client.PatchAsync($"/api/partner-accounts/{id}/balance", JsonContent.Create(payload));
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors")
            .GetProperty("newBalance")[0].GetString()
            .Should().Be("Balance cannot be negative");

        _repoMock.Verify(r => r.UpdatePartnerAccountAsync(It.IsAny<PartnerAccount>(),
                                                         It.IsAny<CancellationToken>()),
                         Times.Never);
    }
}