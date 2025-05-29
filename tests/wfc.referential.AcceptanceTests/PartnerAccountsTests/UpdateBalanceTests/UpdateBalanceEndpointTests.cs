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
using wfc.referential.Domain.ParamTypeAggregate;
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

                // Updated to use BaseRepository methods
                _repoMock.Setup(r => r.Update(It.IsAny<PartnerAccount>()));
                _repoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

                services.AddSingleton(_repoMock.Object);
                services.AddSingleton(cacheMock.Object);
            });
        });

        _client = customisedFactory.CreateClient();
    }

    private static PartnerAccount CreateTestPartnerAccount(Guid id, string accountNumber, string rib, decimal balance)
    {
        var bankId = Guid.NewGuid();
        var bank = Bank.Create(BankId.Of(bankId), "AWB", "Attijariwafa Bank", "AWB");

        var accountTypeId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var accountType = ParamType.Create(ParamTypeId.Of(accountTypeId), null, "Activity");

        return PartnerAccount.Create(
            PartnerAccountId.Of(id),
            accountNumber,
            rib,
            "Casablanca Centre",
            "Test Business",
            "TB",
            balance,
            bank,
            accountType
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
        _repoMock.Setup(r => r.Update(It.IsAny<PartnerAccount>()))
                 .Callback<PartnerAccount>(p => updated = p);

        var payload = new
        {
            PartnerAccountId = id,
            NewBalance = 75000.00m
        };

        // Act
        var response = await _client.PatchAsync($"/api/partner-accounts/{id}/balance", JsonContent.Create(payload));
        var returned = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returned.Should().BeTrue();

        updated!.AccountBalance.Should().Be(75000.00m);
        updated.AccountNumber.Should().Be("000123456789");
        updated.RIB.Should().Be("12345678901234567890123");

        _repoMock.Verify(r => r.Update(It.IsAny<PartnerAccount>()), Times.Once);
        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}