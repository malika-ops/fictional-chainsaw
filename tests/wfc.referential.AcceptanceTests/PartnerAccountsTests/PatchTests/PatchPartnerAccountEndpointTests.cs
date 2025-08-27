using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Moq;
using wfc.referential.Domain.BankAggregate;
using wfc.referential.Domain.PartnerAccountAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.PartnerAccountsTests.PatchTests;

public class PatchPartnerAccountEndpointTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{
    private static PartnerAccount CreateTestPartnerAccount(Guid id, string accountNumber, string rib, string businessName)
    {
        var bankId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var bank = Bank.Create(BankId.Of(bankId), "AWB", "Attijariwafa Bank", "AWB");

        return PartnerAccount.Create(
            PartnerAccountId.Of(id),
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

    [Fact(DisplayName = "PATCH /api/partner-accounts/{id} returns 200 and patches only the provided fields")]
    public async Task Patch_ShouldReturn200_AndPatchOnlyProvidedFields()
    {
        // Arrange
        var id = Guid.NewGuid();
        var partnerAccount = CreateTestPartnerAccount(id, "000123456789", "12345678901234567890123", "Old Business");

        _partnerAccountRepoMock.Setup(r => r.GetByIdAsync(It.Is<PartnerAccountId>(pid => pid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(partnerAccount);

        _partnerAccountRepoMock.Setup(r => r.GetOneByConditionAsync(
            It.Is<System.Linq.Expressions.Expression<System.Func<PartnerAccount, bool>>>(
                expr => expr.Compile().Invoke(partnerAccount) == false), // Different account number
            It.IsAny<CancellationToken>()))
                 .ReturnsAsync((PartnerAccount?)null);

        PartnerAccount? updated = null;
        _partnerAccountRepoMock.Setup(r => r.Update(It.IsAny<PartnerAccount>()))
                 .Callback<PartnerAccount>(p => updated = p);

        var payload = new
        {
            PartnerAccountId = id,
            AccountNumber = "000987654321",
            AccountBalance = 75000.00m
        };

        // Act
        var response = await _client.PatchAsync($"/api/partner-accounts/{id}", JsonContent.Create(payload));
        var returned = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returned.Should().BeTrue();

        updated!.AccountNumber.Should().Be("000987654321");
        updated.AccountBalance.Should().Be(75000.00m);
        updated.RIB.Should().Be("12345678901234567890123");
        updated.BusinessName.Should().Be("Old Business");

        _partnerAccountRepoMock.Verify(r => r.Update(It.IsAny<PartnerAccount>()), Times.Once);
        _partnerAccountRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}