using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Moq;
using wfc.referential.Domain.BankAggregate;
using wfc.referential.Domain.ParamTypeAggregate;
using wfc.referential.Domain.PartnerAccountAggregate;
using wfc.referential.Domain.TypeDefinitionAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.PartnerAccountsTests.GetByIdTests;

public class GetPartnerAccountByIdEndpointTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{
    private static PartnerAccount Make(Guid id, string code = "PARTNER-ACCOUNT-001", string? name = null, bool enabled = true)
    {
        var bank = Bank.Create(
            id: BankId.Of(Guid.NewGuid()),
            code: "BANK-001",
            name: "Test Bank",
            abbreviation: "TST"
        );

        var accountType = ParamType.Create(
            paramTypeId: ParamTypeId.Of(Guid.NewGuid()),
            typeDefinitionId: TypeDefinitionId.Of(Guid.NewGuid()),
            value: "Checking Account"
        );

        var partnerAccount = PartnerAccount.Create(
            id: PartnerAccountId.Of(id),
            accountNumber: code,
            rib: $"RIB-{code}",
            domiciliation: "Test Domiciliation",
            businessName: name ?? $"PartnerAccount-{code}",
            shortName: code.Substring(0, Math.Min(10, code.Length)),
            accountBalance: 1000.00m,
            bank: bank,
            accountType: accountType
        );

        if (!enabled)
            partnerAccount.Disable();

        return partnerAccount;
    }

    private record PartnerAccountDto(Guid Id, string AccountNumber, string BusinessName, bool IsEnabled);

    [Fact(DisplayName = "GET /api/partner-accounts/{id} → 404 when PartnerAccount not found")]
    public async Task Get_ShouldReturn404_WhenPartnerAccountNotFound()
    {
        var id = Guid.NewGuid();

        _partnerAccountRepoMock.Setup(r => r.GetByIdAsync(PartnerAccountId.Of(id), It.IsAny<CancellationToken>()))
             .ReturnsAsync((PartnerAccount?)null);

        var res = await _client.GetAsync($"/api/partner-accounts/{id}");
        var doc = await res.Content.ReadFromJsonAsync<JsonDocument>();

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var root = doc!.RootElement;
        root.GetProperty("title").GetString().Should().Be("Resource Not Found");
        root.GetProperty("status").GetInt32().Should().Be(404);

        _partnerAccountRepoMock.Verify(r => r.GetByIdAsync(PartnerAccountId.Of(id), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "GET /api/partner-accounts/{id} → 404 when id is malformed")]
    public async Task Get_ShouldReturn404_WhenIdMalformed()
    {
        const string bad = "not-a-guid";

        var res = await _client.GetAsync($"/api/partner-accounts/{bad}");

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);

        _partnerAccountRepoMock.Verify(r => r.GetByIdAsync(It.IsAny<PartnerAccountId>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "GET /api/partner-accounts/{id} → 200 for disabled PartnerAccount")]
    public async Task Get_ShouldReturn200_WhenPartnerAccountDisabled()
    {
        var id = Guid.NewGuid();
        var entity = Make(id, "PARTNER-ACCOUNT-DIS", enabled: false);

        _partnerAccountRepoMock.Setup(r => r.GetByIdAsync(PartnerAccountId.Of(id), It.IsAny<CancellationToken>()))
             .ReturnsAsync(entity);

        var res = await _client.GetAsync($"/api/partner-accounts/{id}");
        var dto = await res.Content.ReadFromJsonAsync<PartnerAccountDto>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.IsEnabled.Should().BeFalse();
    }
}