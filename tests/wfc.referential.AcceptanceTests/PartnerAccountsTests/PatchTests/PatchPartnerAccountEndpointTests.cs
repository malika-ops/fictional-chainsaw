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

namespace wfc.referential.AcceptanceTests.PartnerAccountsTests.PatchTests;

public class PatchPartnerAccountEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IPartnerAccountRepository> _repoMock = new();
    private readonly Mock<IBankRepository> _bankRepoMock = new();
    private readonly Mock<IParamTypeRepository> _paramTypeRepoMock = new();

    public PatchPartnerAccountEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var customisedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IPartnerAccountRepository>();
                services.RemoveAll<IBankRepository>();
                services.RemoveAll<IParamTypeRepository>();
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

                // Set up param type mock to return valid account types
                var activityTypeId = ParamTypeId.Of(Guid.Parse("22222222-2222-2222-2222-222222222222"));
                var commissionTypeId = ParamTypeId.Of(Guid.Parse("33333333-3333-3333-3333-333333333333"));

                var activityType = ParamType.Create(activityTypeId, null, "Activity");
                var commissionType = ParamType.Create(commissionTypeId, null, "Commission");

                _paramTypeRepoMock
                    .Setup(r => r.GetByIdAsync(It.Is<ParamTypeId>(id => id.Value == activityTypeId.Value), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(activityType);

                _paramTypeRepoMock
                    .Setup(r => r.GetByIdAsync(It.Is<ParamTypeId>(id => id.Value == commissionTypeId.Value), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(commissionType);

                services.AddSingleton(_repoMock.Object);
                services.AddSingleton(_bankRepoMock.Object);
                services.AddSingleton(_paramTypeRepoMock.Object);
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

        var accountTypeId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var accountType = ParamType.Create(ParamTypeId.Of(accountTypeId), null, "Activity");

        return PartnerAccount.Create(
            new PartnerAccountId(id),
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

    [Fact(DisplayName = "PATCH /api/partner-accounts/{id} returns 200 and patches only the provided fields")]
    public async Task Patch_ShouldReturn200_AndPatchOnlyProvidedFields()
    {
        // Arrange
        var id = Guid.NewGuid();
        var partnerAccount = CreateTestPartnerAccount(id, "000123456789", "12345678901234567890123", "Old Business");

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<PartnerAccountId>(pid => pid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(partnerAccount);

        _repoMock.Setup(r => r.GetByAccountNumberAsync("000987654321", It.IsAny<CancellationToken>()))
                 .ReturnsAsync((PartnerAccount?)null);   // Account number is unique

        PartnerAccount? updated = null;
        _repoMock.Setup(r => r.UpdatePartnerAccountAsync(It.IsAny<PartnerAccount>(),
                                                        It.IsAny<CancellationToken>()))
                 .Callback<PartnerAccount, CancellationToken>((p, _) => updated = p)
                 .Returns(Task.CompletedTask);

        var payload = new
        {
            PartnerAccountId = id,
            AccountNumber = "000987654321",
            // Other fields intentionally omitted - should not change
            AccountBalance = 75000.00m // Including AccountBalance
        };

        // Act
        var response = await _client.PatchAsync($"/api/partner-accounts/{id}", JsonContent.Create(payload));
        var returned = await response.Content.ReadFromJsonAsync<Guid>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returned.Should().Be(id);

        updated!.AccountNumber.Should().Be("000987654321");  // Should change
        updated.AccountBalance.Should().Be(75000.00m);      // Should change
        updated.RIB.Should().Be("12345678901234567890123"); // Should not change
        updated.BusinessName.Should().Be("Old Business");   // Should not change
        updated.AccountTypeId.Value.Should().Be(Guid.Parse("22222222-2222-2222-2222-222222222222")); // Should not change
        updated.IsEnabled.Should().BeTrue(); // Should remain enabled

        _repoMock.Verify(r => r.UpdatePartnerAccountAsync(It.IsAny<PartnerAccount>(),
                                                         It.IsAny<CancellationToken>()),
                          Times.Once);
    }

    [Fact(DisplayName = "PATCH /api/partner-accounts/{id} allows changing only the enabled status")]
    public async Task Patch_ShouldAllowChangingOnlyEnabledStatus()
    {
        // Arrange
        var id = Guid.NewGuid();
        var partnerAccount = CreateTestPartnerAccount(id, "000123456789", "12345678901234567890123", "Test Business");

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
            IsEnabled = false // Change from enabled to disabled
            // Other fields intentionally omitted
        };

        // Act
        var response = await _client.PatchAsync($"/api/partner-accounts/{id}", JsonContent.Create(payload));
        var returned = await response.Content.ReadFromJsonAsync<Guid>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returned.Should().Be(id);

        updated!.IsEnabled.Should().BeFalse(); // Should be disabled
        updated.AccountNumber.Should().Be("000123456789"); // Should not change
        updated.BusinessName.Should().Be("Test Business"); // Should not change

        _repoMock.Verify(r => r.UpdatePartnerAccountAsync(It.IsAny<PartnerAccount>(),
                                                         It.IsAny<CancellationToken>()),
                          Times.Once);
    }

    [Fact(DisplayName = "PATCH /api/partner-accounts/{id} returns 400 when account doesn't exist")]
    public async Task Patch_ShouldReturn400_WhenAccountDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<PartnerAccountId>(pid => pid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((PartnerAccount?)null);

        var payload = new
        {
            PartnerAccountId = id,
            AccountBalance = 75000.00m
        };

        // Act
        var response = await _client.PatchAsync($"/api/partner-accounts/{id}", JsonContent.Create(payload));
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors").GetString()
           .Should().Be("Partner account not found");

        _repoMock.Verify(r => r.UpdatePartnerAccountAsync(It.IsAny<PartnerAccount>(),
                                                         It.IsAny<CancellationToken>()),
                         Times.Never);
    }

    [Fact(DisplayName = "PATCH /api/partner-accounts/{id} returns 400 when new account number already exists")]
    public async Task Patch_ShouldReturn400_WhenNewAccountNumberAlreadyExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existingId = Guid.NewGuid();

        var existing = CreateTestPartnerAccount(existingId, "000555666777", "55566677788899900011122", "Existing Business");
        var target = CreateTestPartnerAccount(id, "000123456789", "12345678901234567890123", "Target Business");

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<PartnerAccountId>(pid => pid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(target);

        _repoMock.Setup(r => r.GetByAccountNumberAsync("000555666777", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(existing); // Duplicate account number

        var payload = new
        {
            PartnerAccountId = id,
            AccountNumber = "000555666777"  // This account number already exists for another account
        };

        // Act
        var response = await _client.PatchAsync($"/api/partner-accounts/{id}", JsonContent.Create(payload));
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors").GetString()
           .Should().Be("Partner account with account number 000555666777 already exists.");

        _repoMock.Verify(r => r.UpdatePartnerAccountAsync(It.IsAny<PartnerAccount>(),
                                                         It.IsAny<CancellationToken>()),
                         Times.Never);
    }

    [Fact(DisplayName = "PATCH /api/partner-accounts/{id} returns 400 when provided Bank doesn't exist")]
    public async Task Patch_ShouldReturn400_WhenProvidedBankDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();
        var nonExistentBankId = Guid.Parse("99999999-9999-9999-9999-999999999999");

        var partnerAccount = CreateTestPartnerAccount(id, "000123456789", "12345678901234567890123", "Test Business");

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<PartnerAccountId>(pid => pid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(partnerAccount);

        // Setup bank repository to return null for this ID
        _bankRepoMock
            .Setup(r => r.GetByIdAsync(It.Is<BankId>(bid => bid.Value == nonExistentBankId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Bank?)null);

        var payload = new
        {
            PartnerAccountId = id,
            BankId = nonExistentBankId
        };

        // Act
        var response = await _client.PatchAsync($"/api/partner-accounts/{id}", JsonContent.Create(payload));
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors").GetString()
           .Should().Be($"Bank with ID {nonExistentBankId} not found");

        _repoMock.Verify(r => r.UpdatePartnerAccountAsync(It.IsAny<PartnerAccount>(),
                                                         It.IsAny<CancellationToken>()),
                         Times.Never);
    }

    [Fact(DisplayName = "PATCH /api/partner-accounts/{id} returns 400 when provided AccountType doesn't exist")]
    public async Task Patch_ShouldReturn400_WhenProvidedAccountTypeDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();
        var nonExistentAccountTypeId = Guid.Parse("99999999-9999-9999-9999-999999999999");

        var partnerAccount = CreateTestPartnerAccount(id, "000123456789", "12345678901234567890123", "Test Business");

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<PartnerAccountId>(pid => pid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(partnerAccount);

        // Setup param type repository to return null for this ID
        _paramTypeRepoMock
            .Setup(r => r.GetByIdAsync(It.Is<ParamTypeId>(pid => pid.Value == nonExistentAccountTypeId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ParamType?)null);

        var payload = new
        {
            PartnerAccountId = id,
            AccountTypeId = nonExistentAccountTypeId
        };

        // Act
        var response = await _client.PatchAsync($"/api/partner-accounts/{id}", JsonContent.Create(payload));
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors").GetString()
           .Should().Be($"Account Type with ID {nonExistentAccountTypeId} not found");

        _repoMock.Verify(r => r.UpdatePartnerAccountAsync(It.IsAny<PartnerAccount>(),
                                                         It.IsAny<CancellationToken>()),
                         Times.Never);
    }
}