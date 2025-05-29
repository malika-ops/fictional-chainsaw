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

namespace wfc.referential.AcceptanceTests.PartnerAccountsTests.UpdateTests;

public class UpdatePartnerAccountEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IPartnerAccountRepository> _repoMock = new();
    private readonly Mock<IBankRepository> _bankRepoMock = new();
    private readonly Mock<IParamTypeRepository> _paramTypeRepoMock = new();

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
                services.RemoveAll<IParamTypeRepository>();
                services.RemoveAll<ICacheService>();

                // Updated to use BaseRepository methods
                _repoMock.Setup(r => r.Update(It.IsAny<PartnerAccount>()));
                _repoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
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
            PartnerAccountId.Of(id), // Updated to use factory method
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

    [Fact(DisplayName = "PUT /api/partner-accounts/{id} returns 200 when update succeeds")]
    public async Task Put_ShouldReturn200_WhenUpdateIsSuccessful()
    {
        // Arrange
        var id = Guid.NewGuid();
        var bankId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var activityTypeId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var commissionTypeId = Guid.Parse("33333333-3333-3333-3333-333333333333");

        var oldAccount = CreateTestPartnerAccount(id, "000123456789", "12345678901234567890123", "Old Business");

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<PartnerAccountId>(pid => pid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(oldAccount);

        // Use GetOneByConditionAsync for uniqueness checks
        _repoMock.Setup(r => r.GetOneByConditionAsync(
            It.Is<System.Linq.Expressions.Expression<System.Func<PartnerAccount, bool>>>(
                expr => expr.Compile().Invoke(oldAccount) == false), // Different account number
            It.IsAny<CancellationToken>()))
                 .ReturnsAsync((PartnerAccount?)null);   // Account number is unique

        PartnerAccount? updated = null;
        _repoMock.Setup(r => r.Update(It.IsAny<PartnerAccount>()))
                 .Callback<PartnerAccount>(p => updated = p);

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
            AccountTypeId = commissionTypeId,
            IsEnabled = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/partner-accounts/{id}", payload);
        var result = await response.Content.ReadFromJsonAsync<bool>(); // Now returns bool

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeTrue(); // Updated assertion

        updated!.AccountNumber.Should().Be("000987654321");
        updated.RIB.Should().Be("98765432109876543210987");
        updated.BusinessName.Should().Be("New Business Name");
        updated.ShortName.Should().Be("NBN");
        updated.AccountBalance.Should().Be(75000.00m);
        updated.AccountTypeId.Value.Should().Be(commissionTypeId);
        updated.IsEnabled.Should().BeTrue();

        _repoMock.Verify(r => r.Update(It.IsAny<PartnerAccount>()), Times.Once);
        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PUT /api/partner-accounts/{id} allows changing the enabled status")]
    public async Task Put_ShouldAllowChangingEnabledStatus_WhenUpdateIsSuccessful()
    {
        // Arrange
        var id = Guid.NewGuid();
        var bankId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var accountTypeId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        var oldAccount = CreateTestPartnerAccount(id, "000123456789", "12345678901234567890123", "Test Bank");

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<PartnerAccountId>(pid => pid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(oldAccount);

        // Same account - no uniqueness violation
        _repoMock.Setup(r => r.GetOneByConditionAsync(
            It.IsAny<System.Linq.Expressions.Expression<System.Func<PartnerAccount, bool>>>(),
            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(oldAccount);   // Same account is ok

        PartnerAccount? updated = null;
        _repoMock.Setup(r => r.Update(It.IsAny<PartnerAccount>()))
                 .Callback<PartnerAccount>(p => updated = p);

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
            AccountTypeId = accountTypeId,
            IsEnabled = false // Changed from true to false
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/partner-accounts/{id}", payload);
        var result = await response.Content.ReadFromJsonAsync<bool>(); // Now returns bool

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeTrue(); // Updated assertion

        updated!.IsEnabled.Should().BeFalse();

        _repoMock.Verify(r => r.Update(It.IsAny<PartnerAccount>()), Times.Once);
        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PUT /api/partner-accounts/{id} returns 400 when AccountNumber is missing")]
    public async Task Put_ShouldReturn400_WhenAccountNumberMissing()
    {
        // Arrange
        var id = Guid.NewGuid();
        var bankId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var accountTypeId = Guid.Parse("22222222-2222-2222-2222-222222222222");

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
            AccountTypeId = accountTypeId,
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

        _repoMock.Verify(r => r.Update(It.IsAny<PartnerAccount>()), Times.Never);
        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PUT /api/partner-accounts/{id} returns 409 when new account number already exists")]
    public async Task Put_ShouldReturn409_WhenAccountNumberAlreadyExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existingId = Guid.NewGuid();
        var bankId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var accountTypeId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        var existing = CreateTestPartnerAccount(existingId, "000555666777", "55566677788899900011122", "Existing Business");
        var target = CreateTestPartnerAccount(id, "000123456789", "12345678901234567890123", "Target Business");

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<PartnerAccountId>(pid => pid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(target);

        // Mock for duplicate account number check
        _repoMock.Setup(r => r.GetOneByConditionAsync(
            It.IsAny<System.Linq.Expressions.Expression<System.Func<PartnerAccount, bool>>>(),
            It.IsAny<CancellationToken>()))
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
            AccountTypeId = accountTypeId,
            IsEnabled = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/partner-accounts/{id}", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict); // 409 for duplicates

        _repoMock.Verify(r => r.Update(It.IsAny<PartnerAccount>()), Times.Never);
        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PUT /api/partner-accounts/{id} returns 400 when account doesn't exist")]
    public async Task Put_ShouldReturn400_WhenAccountDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();
        var bankId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var accountTypeId = Guid.Parse("22222222-2222-2222-2222-222222222222");

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
            AccountTypeId = accountTypeId,
            IsEnabled = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/partner-accounts/{id}", payload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors").GetString()
           .Should().Be($"Partner account with ID {id} not found");

        _repoMock.Verify(r => r.Update(It.IsAny<PartnerAccount>()), Times.Never);
        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PUT /api/partner-accounts/{id} returns 400 when bank doesn't exist")]
    public async Task Put_ShouldReturn400_WhenBankDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();
        var nonExistentBankId = Guid.Parse("99999999-9999-9999-9999-999999999999");
        var accountTypeId = Guid.Parse("22222222-2222-2222-2222-222222222222");

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
            AccountTypeId = accountTypeId,
            IsEnabled = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/partner-accounts/{id}", payload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors").GetString()
           .Should().Be($"Bank with ID {nonExistentBankId} not found");

        _repoMock.Verify(r => r.Update(It.IsAny<PartnerAccount>()), Times.Never);
        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PUT /api/partner-accounts/{id} returns 400 when account type doesn't exist")]
    public async Task Put_ShouldReturn400_WhenAccountTypeDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();
        var bankId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var nonExistentAccountTypeId = Guid.Parse("99999999-9999-9999-9999-999999999999");

        var account = CreateTestPartnerAccount(id, "000123456789", "12345678901234567890123", "Test Business");

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<PartnerAccountId>(pid => pid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(account);

        // Setup param type repository to return null for this ID
        _paramTypeRepoMock
            .Setup(r => r.GetByIdAsync(It.Is<ParamTypeId>(pid => pid.Value == nonExistentAccountTypeId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ParamType?)null);

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
            AccountTypeId = nonExistentAccountTypeId,
            IsEnabled = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/partner-accounts/{id}", payload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors").GetString()
           .Should().Be($"Account Type with ID {nonExistentAccountTypeId} not found");

        _repoMock.Verify(r => r.Update(It.IsAny<PartnerAccount>()), Times.Never);
        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}