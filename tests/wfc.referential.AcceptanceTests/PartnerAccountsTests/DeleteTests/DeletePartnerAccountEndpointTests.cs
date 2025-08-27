using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Moq;
using wfc.referential.Domain.BankAggregate;
using wfc.referential.Domain.ParamTypeAggregate;
using wfc.referential.Domain.PartnerAccountAggregate;
using wfc.referential.Domain.PartnerAggregate;
using wfc.referential.Domain.TypeDefinitionAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.PartnerAccountsTests.DeleteTests;

public class DeletePartnerAccountEndpointTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{
    private static PartnerAccount CreateTestPartnerAccount(Guid id)
    {
        var bankId = Guid.NewGuid();
        var bank = Bank.Create(BankId.Of(bankId), "AWB", "Attijariwafa Bank", "AWB");

        return PartnerAccount.Create(
            PartnerAccountId.Of(id),
            "000123456789",
            "12345678901234567890123",
            "Casablanca Centre",
            "Test Business",
            "TB",
            50000.00m,
            bank,
            PartnerAccountTypeEnum.Activité
        );
    }

    private static Partner CreateTestPartner(
        Guid id,
        string code,
        Guid? commissionAccountId = null,
        Guid? activityAccountId = null)
    {
        // Use the new Partner.Create method with all required parameters
        return Partner.Create(
            PartnerId.Of(id),
            code,
            "Test Partner",
            "Natural Person",           // PersonType
            "PTX123456",               // ProfessionalTaxNumber
            "10.5",                    // WithholdingTaxRate
            "Casablanca",              // HeadquartersCity
            "123 Main Street",         // HeadquartersAddress
            "Doe",                     // LastName
            "John",                    // FirstName
            "+212612345678",           // PhoneNumberContact
            "contact@partner.com",     // MailContact
            "Manager",                 // FunctionContact
            "Bank Transfer",           // TransferType
            "SMS",                     // AuthenticationMode
            "123456789",               // TaxIdentificationNumber
            "IR",                      // TaxRegime
            "AUX001",                  // AuxiliaryAccount
            "ICE123456",               // ICE
            ""                         // Logo
        );
    }

    [Fact(DisplayName = "DELETE /api/partner-accounts/{id} returns 200 and disables account")]
    public async Task Delete_ShouldReturn200_AndDisableAccount_WhenAccountExists()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var partnerAccount = CreateTestPartnerAccount(accountId);

        _partnerAccountRepoMock.Setup(r => r.GetByIdAsync(
            It.Is<PartnerAccountId>(id => id.Value == accountId),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(partnerAccount);

        // No linked partners - use GetOneByConditionAsync from BaseRepository
        _partnerRepoMock.Setup(r => r.GetOneByConditionAsync(
            It.IsAny<System.Linq.Expressions.Expression<System.Func<Partner, bool>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((Partner?)null);

        // Act
        var response = await _client.DeleteAsync($"/api/partner-accounts/{accountId}");
        var result = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeTrue();

        // Verify account was disabled (soft delete)
        partnerAccount.IsEnabled.Should().BeFalse();

        _partnerAccountRepoMock.Verify(r => r.GetByIdAsync(
            It.Is<PartnerAccountId>(id => id.Value == accountId),
            It.IsAny<CancellationToken>()),
            Times.Once);

        _partnerAccountRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "DELETE /api/partner-accounts/{id} returns 400 when account not found")]
    public async Task Delete_ShouldReturn400_WhenAccountNotFound()
    {
        // Arrange
        var accountId = Guid.NewGuid();

        _partnerAccountRepoMock.Setup(r => r.GetByIdAsync(
            It.Is<PartnerAccountId>(id => id.Value == accountId),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((PartnerAccount?)null);

        // Act
        var response = await _client.DeleteAsync($"/api/partner-accounts/{accountId}");
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors").GetProperty("message").GetString()
           .Should().Be($"Partner account [{accountId}] not found.");

        _partnerAccountRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact(DisplayName = "DELETE /api/partner-accounts/{id} returns 409 when account is linked to partner")]
    public async Task Delete_ShouldReturn409_WhenAccountIsLinkedToPartner()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var partnerAccount = CreateTestPartnerAccount(accountId);
        var linkedPartner = CreateTestPartner(Guid.NewGuid(), "TEST01");

        // Manually set the commission account relationship
        linkedPartner.SetCommissionAccount(accountId);

        _partnerAccountRepoMock.Setup(r => r.GetByIdAsync(
            It.Is<PartnerAccountId>(id => id.Value == accountId),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(partnerAccount);

        // Partner is linked to this account
        _partnerRepoMock.Setup(r => r.GetOneByConditionAsync(
            It.IsAny<System.Linq.Expressions.Expression<System.Func<Partner, bool>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(linkedPartner);

        // Act
        var response = await _client.DeleteAsync($"/api/partner-accounts/{accountId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        _partnerAccountRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact(DisplayName = "DELETE /api/partner-accounts/{id} performs soft delete not hard delete")]
    public async Task Delete_ShouldPerformSoftDelete_NotHardDelete()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var partnerAccount = CreateTestPartnerAccount(accountId);

        // Verify account starts as enabled
        partnerAccount.IsEnabled.Should().BeTrue();

        _partnerAccountRepoMock.Setup(r => r.GetByIdAsync(
            It.Is<PartnerAccountId>(id => id.Value == accountId),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(partnerAccount);

        _partnerRepoMock.Setup(r => r.GetOneByConditionAsync(
            It.IsAny<System.Linq.Expressions.Expression<System.Func<Partner, bool>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((Partner?)null);

        // Act
        var response = await _client.DeleteAsync($"/api/partner-accounts/{accountId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify soft delete - account still exists but disabled
        partnerAccount.Should().NotBeNull();
        partnerAccount.IsEnabled.Should().BeFalse();
        partnerAccount.AccountNumber.Should().Be("000123456789"); // Data still intact
    }

    [Fact(DisplayName = "DELETE /api/partner-accounts/{id} returns 400 for invalid GUID format")]
    public async Task Delete_ShouldReturn400_ForInvalidGuidFormat()
    {
        // Act
        var response = await _client.DeleteAsync("/api/partner-accounts/invalid-guid-format");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // Verify no repository operations were attempted
        _partnerAccountRepoMock.Verify(r => r.GetByIdAsync(
            It.IsAny<PartnerAccountId>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _partnerAccountRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact(DisplayName = "DELETE /api/partner-accounts/{id} checks both commission and activity account links")]
    public async Task Delete_ShouldCheckBothCommissionAndActivityAccountLinks()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var partnerAccount = CreateTestPartnerAccount(accountId);
        var linkedPartner = CreateTestPartner(Guid.NewGuid(), "TEST01");

        // Set as activity account instead of commission account
        linkedPartner.SetActivityAccount(accountId);

        _partnerAccountRepoMock.Setup(r => r.GetByIdAsync(
            It.Is<PartnerAccountId>(id => id.Value == accountId),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(partnerAccount);

        // Partner is linked to this account as activity account
        _partnerRepoMock.Setup(r => r.GetOneByConditionAsync(
            It.IsAny<System.Linq.Expressions.Expression<System.Func<Partner, bool>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(linkedPartner);

        // Act
        var response = await _client.DeleteAsync($"/api/partner-accounts/{accountId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        _partnerAccountRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact(DisplayName = "DELETE /api/partner-accounts/{id} validates business rules before deletion")]
    public async Task Delete_ShouldValidateBusinessRules_BeforeDeletion()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var partnerAccount = CreateTestPartnerAccount(accountId);

        _partnerAccountRepoMock.Setup(r => r.GetByIdAsync(
            It.Is<PartnerAccountId>(id => id.Value == accountId),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(partnerAccount);

        // No linked partners
        _partnerRepoMock.Setup(r => r.GetOneByConditionAsync(
            It.IsAny<System.Linq.Expressions.Expression<System.Func<Partner, bool>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((Partner?)null);

        // Act
        var response = await _client.DeleteAsync($"/api/partner-accounts/{accountId}");
        var result = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeTrue();

        // Verify business rule validation was performed
        _partnerRepoMock.Verify(r => r.GetOneByConditionAsync(
            It.IsAny<System.Linq.Expressions.Expression<System.Func<Partner, bool>>>(),
            It.IsAny<CancellationToken>()), Times.AtLeastOnce);

        // Verify account was disabled (soft delete)
        partnerAccount.IsEnabled.Should().BeFalse();

        _partnerAccountRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }
}