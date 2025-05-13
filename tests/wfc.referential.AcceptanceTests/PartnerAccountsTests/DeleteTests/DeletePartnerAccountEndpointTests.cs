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

namespace wfc.referential.AcceptanceTests.PartnerAccountsTests.DeleteTests;

public class DeletePartnerAccountEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IPartnerAccountRepository> _repoMock = new();

    public DeletePartnerAccountEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var customisedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IPartnerAccountRepository>();
                services.RemoveAll<ICacheService>();

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

    // Helper to build dummy partner accounts quickly
    private static PartnerAccount CreateTestPartnerAccount(Guid id, string accountNumber, string rib, string businessName)
    {
        var bankId = Guid.NewGuid();
        var bank = Bank.Create(BankId.Of(bankId), "AWB", "Attijariwafa Bank", "AWB");

        return PartnerAccount.Create(
            new PartnerAccountId(id),
            accountNumber,
            rib,
            "Casablanca Centre",
            businessName,
            businessName.Substring(0, 2).ToUpper(),
            50000.00m,
            bank,
            AccountType.Activité
        );
    }

    [Fact(DisplayName = "DELETE /api/partner-accounts/{id} returns 200 when account exists")]
    public async Task Delete_ShouldReturn200_WhenAccountExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var partnerAccount = CreateTestPartnerAccount(
            id,
            "000123456789",
            "12345678901234567890123",
            "Test Business"
        );

        _repoMock
            .Setup(r => r.GetByIdAsync(It.Is<PartnerAccountId>(pid => pid.Value == id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(partnerAccount);

        // Capture the entity passed to Update
        PartnerAccount? updatedAccount = null;
        _repoMock
            .Setup(r => r.UpdatePartnerAccountAsync(It.IsAny<PartnerAccount>(), It.IsAny<CancellationToken>()))
            .Callback<PartnerAccount, CancellationToken>((p, _) => updatedAccount = p)
            .Returns(Task.CompletedTask);

        // Act
        var response = await _client.DeleteAsync($"/api/partner-accounts/{id}");
        var body = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body.Should().BeTrue();

        updatedAccount!.IsEnabled.Should().BeFalse();

        _repoMock.Verify(r => r.UpdatePartnerAccountAsync(It.IsAny<PartnerAccount>(),
                                                         It.IsAny<CancellationToken>()),
                                                         Times.Once);
    }

    [Fact(DisplayName = "DELETE /api/partner-accounts/{id} returns 400 when account is not found")]
    public async Task Delete_ShouldReturn400_WhenAccountNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();

        _repoMock
            .Setup(r => r.GetByIdAsync(It.Is<PartnerAccountId>(pid => pid.Value == id), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PartnerAccount?)null);

        // Act
        var response = await _client.DeleteAsync($"/api/partner-accounts/{id}");
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors").GetString()
           .Should().Be("Partner account not found");

        _repoMock.Verify(r => r.UpdatePartnerAccountAsync(It.IsAny<PartnerAccount>(),
                                                         It.IsAny<CancellationToken>()),
                                                         Times.Never);
    }
}