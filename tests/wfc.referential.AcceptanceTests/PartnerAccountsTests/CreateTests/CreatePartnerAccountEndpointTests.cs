using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Moq;
using wfc.referential.Domain.BankAggregate;
using wfc.referential.Domain.PartnerAccountAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.PartnerAccountsTests.CreateTests;

public class CreatePartnerAccountEndpointTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{
    [Fact(DisplayName = "POST /api/partner-accounts returns 200 and Guid when request is valid")]
    public async Task Post_ShouldReturn200_AndId_WhenRequestIsValid()
    {
        // Arrange
        var id = Guid.Parse("11111111-1111-1111-1111-111111111111");

        var payload = new
        {
            AccountNumber = "000123456789",
            RIB = "12345678901234567890123",
            Domiciliation = "Casablanca Centre",
            BusinessName = "Wafa Cash Services",
            ShortName = "WCS",
            AccountBalance = 50000.00m,
            BankId = id,
            PartnerAccountType = "1",
        };

        // Set up bank mock to return valid entities
        var bankId = BankId.Of(id);
        _bankRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<BankId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Bank.Create(bankId, "AWB", "Attijariwafa Bank", "AWB"));

        // Set up the mock to return null for duplicate checks initially
        _partnerAccountRepoMock
            .Setup(r => r.GetOneByConditionAsync(
                It.IsAny<System.Linq.Expressions.Expression<System.Func<PartnerAccount, bool>>>(),
                It.IsAny<CancellationToken>()));

        // Act
        var response = await _client.PostAsJsonAsync("/api/partner-accounts", payload);
        var returnedId = await response.Content.ReadFromJsonAsync<Guid>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returnedId.Should().NotBeEmpty();

        _partnerAccountRepoMock.Verify(r =>
            r.AddAsync(It.Is<PartnerAccount>(p =>
                    p.AccountNumber == payload.AccountNumber &&
                    p.RIB == payload.RIB &&
                    p.BusinessName == payload.BusinessName &&
                    p.ShortName == payload.ShortName &&
                    p.AccountBalance == payload.AccountBalance &&
                    p.Bank.Id.Value == id &&
                    p.PartnerAccountType == PartnerAccountTypeEnum.Commission),
                    It.IsAny<CancellationToken>()
            ),
            Times.Once
        );

        _partnerAccountRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "POST /api/partner-accounts returns 409 when AccountNumber already exists")]
    public async Task Post_ShouldReturn409_WhenAccountNumberAlreadyExists()
    {
        // Arrange 
        const string duplicateAccountNumber = "000123456789";
        var bankId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var existingBank = Bank.Create(BankId.Of(bankId), "AWB", "Attijariwafa Bank", "AWB");

        // Create a valid TypeDefinitionId instead of passing null
        var existingAccount = PartnerAccount.Create(
            PartnerAccountId.Of(Guid.NewGuid()),
            duplicateAccountNumber,
            "12345678901234567890123",
            "Casablanca Centre",
            "Existing Business",
            "EB",
            50000.00m,
            existingBank,
            PartnerAccountTypeEnum.Activité
        );

        _partnerAccountRepoMock
            .Setup(r => r.GetOneByConditionAsync(
                It.IsAny<System.Linq.Expressions.Expression<System.Func<PartnerAccount, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingAccount);

        var payload = new
        {
            AccountNumber = duplicateAccountNumber,
            RIB = "98765432109876543210987",
            Domiciliation = "Casablanca Marina",
            BusinessName = "Transfert Express",
            ShortName = "TE",
            AccountBalance = 75000.00m,
            BankId = bankId,
            PartnerAccountType = "1",
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/partner-accounts", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        _partnerAccountRepoMock.Verify(r =>
            r.AddAsync(It.IsAny<PartnerAccount>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}