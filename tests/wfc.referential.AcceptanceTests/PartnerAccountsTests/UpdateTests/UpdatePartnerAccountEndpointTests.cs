using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Moq;
using wfc.referential.Domain.BankAggregate;
using wfc.referential.Domain.PartnerAccountAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.PartnerAccountsTests.UpdateTests;

public class UpdatePartnerAccountEndpointTests : BaseAcceptanceTests
{
    public UpdatePartnerAccountEndpointTests(TestWebApplicationFactory factory) : base(factory)
    {
        //Set up bank mock to return valid entities
                   var bankId = BankId.Of(Guid.Parse("11111111-1111-1111-1111-111111111111"));
        _bankRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<BankId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Bank.Create(bankId, "AWB", "Attijariwafa Bank", "AWB"));

    }

    // Helper to create a test partner account
    private static PartnerAccount CreateTestPartnerAccount(Guid id, string accountNumber, string rib, string businessName)
    {
        var bankId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var bank = Bank.Create(BankId.Of(bankId), "AWB", "Attijariwafa Bank", "AWB");


        return PartnerAccount.Create(
            PartnerAccountId.Of(id), // Updated to use factory method
            accountNumber,
            rib,
            "Casablanca Centre",
            businessName,
            businessName.Substring(0, 2).ToUpper(),
            50000.00m,
            bank,
            PartnerAccountTypeEnum.Activité
        );
    }

    [Fact(DisplayName = "PUT /api/partner-accounts/{id} returns 200 when update succeeds")]
    public async Task Put_ShouldReturn200_WhenUpdateIsSuccessful()
    {
        // Arrange
        var id = Guid.NewGuid();
        var bankId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        var oldAccount = CreateTestPartnerAccount(id, "000123456789", "12345678901234567890123", "Old Business");

        _partnerAccountRepoMock.Setup(r => r.GetByIdAsync(It.Is<PartnerAccountId>(pid => pid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(oldAccount);

        // Use GetOneByConditionAsync for uniqueness checks
        _partnerAccountRepoMock.Setup(r => r.GetOneByConditionAsync(
            It.Is<System.Linq.Expressions.Expression<System.Func<PartnerAccount, bool>>>(
                expr => expr.Compile().Invoke(oldAccount) == false), // Different account number
            It.IsAny<CancellationToken>()))
                 .ReturnsAsync((PartnerAccount?)null);   // Account number is unique

        PartnerAccount? updated = null;
        _partnerAccountRepoMock.Setup(r => r.Update(It.IsAny<PartnerAccount>()))
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
            PartnerAccountType = "1",
            IsEnabled = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/partner-accounts/{id}", payload);
        var result = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeTrue(); // Updated assertion

        updated!.AccountNumber.Should().Be("000987654321");
        updated.RIB.Should().Be("98765432109876543210987");
        updated.BusinessName.Should().Be("New Business Name");
        updated.ShortName.Should().Be("NBN");
        updated.AccountBalance.Should().Be(75000.00m);
        updated.IsEnabled.Should().BeTrue();

        _partnerAccountRepoMock.Verify(r => r.Update(It.IsAny<PartnerAccount>()), Times.Once);
        _partnerAccountRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PUT /api/partner-accounts/{id} allows changing the enabled status")]
    public async Task Put_ShouldAllowChangingEnabledStatus_WhenUpdateIsSuccessful()
    {
        // Arrange
        var id = Guid.NewGuid();
        var bankId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        var oldAccount = CreateTestPartnerAccount(id, "000123456789", "12345678901234567890123", "Test Bank");

        _partnerAccountRepoMock.Setup(r => r.GetByIdAsync(It.Is<PartnerAccountId>(pid => pid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(oldAccount);

        // Same account - no uniqueness violation
        _partnerAccountRepoMock.Setup(r => r.GetOneByConditionAsync(
            It.IsAny<System.Linq.Expressions.Expression<System.Func<PartnerAccount, bool>>>(),
            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(oldAccount);   // Same account is ok

        PartnerAccount? updated = null;
        _partnerAccountRepoMock.Setup(r => r.Update(It.IsAny<PartnerAccount>()))
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
            PartnerAccountType = "1",
            IsEnabled = false // Changed from true to false
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/partner-accounts/{id}", payload);
        var result = await response.Content.ReadFromJsonAsync<bool>(); // Now returns bool

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeTrue(); // Updated assertion

        updated!.IsEnabled.Should().BeFalse();

        _partnerAccountRepoMock.Verify(r => r.Update(It.IsAny<PartnerAccount>()), Times.Once);
        _partnerAccountRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
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
            PartnerAccountType = "1",
            IsEnabled = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/partner-accounts/{id}", payload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors")
            .GetProperty("AccountNumber")[0].GetString()
            .Should().Be("Account number is required");

        _partnerAccountRepoMock.Verify(r => r.Update(It.IsAny<PartnerAccount>()), Times.Never);
        _partnerAccountRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PUT /api/partner-accounts/{id} returns 409 when new account number already exists")]
    public async Task Put_ShouldReturn409_WhenAccountNumberAlreadyExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existingId = Guid.NewGuid();
        var bankId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        var existing = CreateTestPartnerAccount(existingId, "000555666777", "55566677788899900011122", "Existing Business");
        var target = CreateTestPartnerAccount(id, "000123456789", "12345678901234567890123", "Target Business");

        _partnerAccountRepoMock.Setup(r => r.GetByIdAsync(It.Is<PartnerAccountId>(pid => pid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(target);

        // Mock for duplicate account number check
        _partnerAccountRepoMock.Setup(r => r.GetOneByConditionAsync(
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
            PartnerAccountType = "1",
            IsEnabled = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/partner-accounts/{id}", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict); // 409 for duplicates

        _partnerAccountRepoMock.Verify(r => r.Update(It.IsAny<PartnerAccount>()), Times.Never);
        _partnerAccountRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PUT /api/partner-accounts/{id} returns 400 when account doesn't exist")]
    public async Task Put_ShouldReturn400_WhenAccountDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();
        var bankId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        _partnerAccountRepoMock.Setup(r => r.GetByIdAsync(It.Is<PartnerAccountId>(pid => pid.Value == id), It.IsAny<CancellationToken>()))
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
            PartnerAccountType = "1",
            IsEnabled = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/partner-accounts/{id}", payload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors").GetProperty("message").GetString()
           .Should().Be($"Partner account with ID {id} not found");

        _partnerAccountRepoMock.Verify(r => r.Update(It.IsAny<PartnerAccount>()), Times.Never);
        _partnerAccountRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PUT /api/partner-accounts/{id} returns 400 when bank doesn't exist")]
    public async Task Put_ShouldReturn400_WhenBankDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();
        var nonExistentBankId = Guid.Parse("99999999-9999-9999-9999-999999999999");

        var account = CreateTestPartnerAccount(id, "000123456789", "12345678901234567890123", "Test Business");

        _partnerAccountRepoMock.Setup(r => r.GetByIdAsync(It.Is<PartnerAccountId>(pid => pid.Value == id), It.IsAny<CancellationToken>()))
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
            PartnerAccountType = "1",
            IsEnabled = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/partner-accounts/{id}", payload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors").GetProperty("message").GetString()
           .Should().Be($"Bank with ID {nonExistentBankId} not found");

        _partnerAccountRepoMock.Verify(r => r.Update(It.IsAny<PartnerAccount>()), Times.Never);
        _partnerAccountRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}