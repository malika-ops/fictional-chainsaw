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

namespace wfc.referential.AcceptanceTests.PartnerAccountsTests.UpdateTests;

public class UpdatePartnerAccountEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IPartnerAccountRepository> _repoMock = new();
    private readonly Mock<IBankRepository> _bankRepoMock = new();

    public UpdatePartnerAccountEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var customisedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IPartnerAccountRepository>();
                services.RemoveAll<IBankRepository>();
                services.RemoveAll<ICacheService>();

                // Default noop for Update
                _repoMock
                    .Setup(r => r.UpdatePartnerAccountAsync(It.IsAny<PartnerAccount>(),
                                                           It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

                // Set up bank mock to return valid entities
                var bankId = BankId.Of(Guid.Parse("11111111-1111-1111-1111-111111111111"));
                _bankRepoMock
                    .Setup(r => r.GetByIdAsync(It.IsAny<BankId>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(Bank.Create(bankId, "AWB", "Attijariwafa Bank", "AWB"));

                services.AddSingleton(_repoMock.Object);
                services.AddSingleton(_bankRepoMock.Object);
                services.AddSingleton(cacheMock.Object);
            });
        });

        _client = customisedFactory.CreateClient();
    }

    // Helper to create a test partner account
    private static PartnerAccount CreateTestPartnerAccount(Guid id, string accountNumber, string rib, string businessName)
    {
        var bankId = Guid.Parse("11111111-1111-1111-1111-111111111111");
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

    [Fact(DisplayName = "PUT /api/partner-accounts/{id} returns 200 when update succeeds")]
    public async Task Put_ShouldReturn200_WhenUpdateIsSuccessful()
    {
        // Arrange
        var id = Guid.NewGuid();
        var bankId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var oldAccount = CreateTestPartnerAccount(id, "000123456789", "12345678901234567890123", "Old Business");

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<PartnerAccountId>(pid => pid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(oldAccount);

        _repoMock.Setup(r => r.GetByAccountNumberAsync("000987654321", It.IsAny<CancellationToken>()))
                 .ReturnsAsync((PartnerAccount?)null);   // Account number is unique

        _repoMock.Setup(r => r.GetByRIBAsync("98765432109876543210987", It.IsAny<CancellationToken>()))
                 .ReturnsAsync((PartnerAccount?)null);   // RIB is unique

        PartnerAccount? updated = null;
        _repoMock.Setup(r => r.UpdatePartnerAccountAsync(It.IsAny<PartnerAccount>(),
                                                It.IsAny<CancellationToken>()))
                 .Callback<PartnerAccount, CancellationToken>((p, _) => updated = p)
                 .Returns(Task.CompletedTask);

        var payload = new
        {
            PartnerAccountId = id,
            AccountNumber = "000987654321",
            RIB = "98765432109876543210987",
            Domiciliation = "Casablanca Marina",
            BusinessName = "New Business Name",
            ShortName = "NBN",
            AccountBalance = 75000.00m,
            BankId = bankId,
            AccountType = "Commission",
            IsEnabled = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/partner-accounts/{id}", payload);
        var returned = await response.Content.ReadFromJsonAsync<Guid>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returned.Should().Be(id);

        updated!.AccountNumber.Should().Be("000987654321");
        updated.RIB.Should().Be("98765432109876543210987");
        updated.BusinessName.Should().Be("New Business Name");
        updated.ShortName.Should().Be("NBN");
        updated.AccountBalance.Should().Be(75000.00m);
        updated.AccountType.Should().Be(AccountType.Commission);
        updated.IsEnabled.Should().BeTrue();

        _repoMock.Verify(r => r.UpdatePartnerAccountAsync(It.IsAny<PartnerAccount>(),
                                                         It.IsAny<CancellationToken>()),
                          Times.Once);
    }

    [Fact(DisplayName = "PUT /api/partner-accounts/{id} allows changing the enabled status")]
    public async Task Put_ShouldAllowChangingEnabledStatus_WhenUpdateIsSuccessful()
    {
        // Arrange
        var id = Guid.NewGuid();
        var bankId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var oldAccount = CreateTestPartnerAccount(id, "000123456789", "12345678901234567890123", "Test Bank");

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<PartnerAccountId>(pid => pid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(oldAccount);

        _repoMock.Setup(r => r.GetByAccountNumberAsync("000123456789", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(oldAccount);   // Same account number is ok for same account

        _repoMock.Setup(r => r.GetByRIBAsync("12345678901234567890123", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(oldAccount);   // Same RIB is ok for same account

        PartnerAccount? updated = null;
        _repoMock.Setup(r => r.UpdatePartnerAccountAsync(It.IsAny<PartnerAccount>(),
                                                It.IsAny<CancellationToken>()))
                 .Callback<PartnerAccount, CancellationToken>((p, _) => updated = p)
                 .Returns(Task.CompletedTask);

        var payload = new
        {
            PartnerAccountId = id,
            AccountNumber = "000123456789",
            RIB = "12345678901234567890123",
            Domiciliation = "Casablanca Centre",
            BusinessName = "Test Bank",
            ShortName = "TB",
            AccountBalance = 50000.00m,
            BankId = bankId,
            AccountType = "Activité",
            IsEnabled = false // Changed from true to false
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/partner-accounts/{id}", payload);
        var returned = await response.Content.ReadFromJsonAsync<Guid>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returned.Should().Be(id);

        updated!.IsEnabled.Should().BeFalse();

        _repoMock.Verify(r => r.UpdatePartnerAccountAsync(It.IsAny<PartnerAccount>(),
                                                         It.IsAny<CancellationToken>()),
                          Times.Once);
    }

    [Fact(DisplayName = "PUT /api/partner-accounts/{id} returns 400 when AccountNumber is missing")]
    public async Task Put_ShouldReturn400_WhenAccountNumberMissing()
    {
        // Arrange
        var id = Guid.NewGuid();
        var bankId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        var payload = new
        {
            PartnerAccountId = id,
            // AccountNumber intentionally omitted
            RIB = "98765432109876543210987",
            Domiciliation = "Casablanca Marina",
            BusinessName = "New Business Name",
            ShortName = "NBN",
            AccountBalance = 75000.00m,
            BankId = bankId,
            AccountType = "Commission",
            IsEnabled = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/partner-accounts/{id}", payload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors")
            .GetProperty("accountNumber")[0].GetString()
            .Should().Be("Account number is required");

        _repoMock.Verify(r => r.UpdatePartnerAccountAsync(It.IsAny<PartnerAccount>(),
                                                         It.IsAny<CancellationToken>()),
                         Times.Never);
    }

    [Fact(DisplayName = "PUT /api/partner-accounts/{id} returns 400 when new account number already exists")]
    public async Task Put_ShouldReturn400_WhenAccountNumberAlreadyExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existingId = Guid.NewGuid();
        var bankId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        var existing = CreateTestPartnerAccount(existingId, "000555666777", "55566677788899900011122", "Existing Business");
        var target = CreateTestPartnerAccount(id, "000123456789", "12345678901234567890123", "Target Business");

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<PartnerAccountId>(pid => pid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(target);

        _repoMock.Setup(r => r.GetByAccountNumberAsync("000555666777", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(existing); // Duplicate account number

        var payload = new
        {
            PartnerAccountId = id,
            AccountNumber = "000555666777",   // Duplicate
            RIB = "98765432109876543210987",
            Domiciliation = "Casablanca Marina",
            BusinessName = "Updated Business",
            ShortName = "UB",
            AccountBalance = 75000.00m,
            BankId = bankId,
            AccountType = "Commission",
            IsEnabled = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/partner-accounts/{id}", payload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors").GetString()
           .Should().Be("Partner account with account number 000555666777 already exists.");

        _repoMock.Verify(r => r.UpdatePartnerAccountAsync(It.IsAny<PartnerAccount>(),
                                                         It.IsAny<CancellationToken>()),
                         Times.Never);
    }

    [Fact(DisplayName = "PUT /api/partner-accounts/{id} returns 404 when account doesn't exist")]
    public async Task Put_ShouldReturn404_WhenAccountDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();
        var bankId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<PartnerAccountId>(pid => pid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((PartnerAccount?)null);

        var payload = new
        {
            PartnerAccountId = id,
            AccountNumber = "000987654321",
            RIB = "98765432109876543210987",
            Domiciliation = "Casablanca Marina",
            BusinessName = "New Business",
            ShortName = "NB",
            AccountBalance = 75000.00m,
            BankId = bankId,
            AccountType = "Commission",
            IsEnabled = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/partner-accounts/{id}", payload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors").GetString()
           .Should().Be($"Partner account with ID {id} not found");

        _repoMock.Verify(r => r.UpdatePartnerAccountAsync(It.IsAny<PartnerAccount>(),
                                                         It.IsAny<CancellationToken>()),
                         Times.Never);
    }

    [Fact(DisplayName = "PUT /api/partner-accounts/{id} returns 400 when bank doesn't exist")]
    public async Task Put_ShouldReturn400_WhenBankDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();
        var nonExistentBankId = Guid.Parse("99999999-9999-9999-9999-999999999999");
        var account = CreateTestPartnerAccount(id, "000123456789", "12345678901234567890123", "Test Business");

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<PartnerAccountId>(pid => pid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(account);

        // Setup bank repository to return null for this ID
        _bankRepoMock
            .Setup(r => r.GetByIdAsync(It.Is<BankId>(bid => bid.Value == nonExistentBankId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Bank?)null);

        var payload = new
        {
            PartnerAccountId = id,
            AccountNumber = "000987654321",
            RIB = "98765432109876543210987",
            Domiciliation = "Casablanca Marina",
            BusinessName = "New Business",
            ShortName = "NB",
            AccountBalance = 75000.00m,
            BankId = nonExistentBankId,
            AccountType = "Commission",
            IsEnabled = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/partner-accounts/{id}", payload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors").GetString()
           .Should().Be($"Bank with ID {nonExistentBankId} not found");

        _repoMock.Verify(r => r.UpdatePartnerAccountAsync(It.IsAny<PartnerAccount>(),
                                                         It.IsAny<CancellationToken>()),
                         Times.Never);
    }
}