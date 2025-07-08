using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Moq;
using wfc.referential.Domain.CorridorAggregate;
using wfc.referential.Domain.ServiceAggregate;
using wfc.referential.Domain.TaxAggregate;
using wfc.referential.Domain.TaxRuleDetailAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.TaxRuleDetailTests.GetByIdTests;

public class GetTaxRuleDetailByIdEndpointTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{

    private static TaxRuleDetail Make(Guid id, ApplicationRule appliedOn = ApplicationRule.Fees, bool enabled = true)
    {
        var corridorId = CorridorId.Of(Guid.NewGuid());
        var taxId = TaxId.Of(Guid.NewGuid());
        var serviceId = ServiceId.Of(Guid.NewGuid());

        var taxRuleDetail = TaxRuleDetail.Create(
            id: TaxRuleDetailsId.Of(id),
            corridorId: corridorId,
            taxId: taxId,
            serviceId: serviceId,
            appliedOn: appliedOn,
            isEnabled: enabled
        );

        if (!enabled)
            taxRuleDetail.SetInactive();

        return taxRuleDetail;
    }

    private record TaxRuleDetailDto(
        Guid Id,
        Guid CorridorId,
        Guid TaxId,
        Guid ServiceId,
        string AppliedOn,
        bool IsEnabled
    );

    [Fact(DisplayName = "GET /api/tax-rule-details/{id} → 404 when TaxRuleDetail not found")]
    public async Task Get_ShouldReturn404_WhenTaxRuleDetailNotFound()
    {
        var id = Guid.NewGuid();

        _taxRuleDetailsRepoMock.Setup(r => r.GetByIdAsync(TaxRuleDetailsId.Of(id), It.IsAny<CancellationToken>()))
             .ReturnsAsync((TaxRuleDetail?)null);

        var res = await _client.GetAsync($"/api/tax-rule-details/{id}");
        var doc = await res.Content.ReadFromJsonAsync<JsonDocument>();

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var root = doc!.RootElement;
        root.GetProperty("title").GetString().Should().Be("Resource Not Found");
        root.GetProperty("status").GetInt32().Should().Be(404);

        _taxRuleDetailsRepoMock.Verify(r => r.GetByIdAsync(TaxRuleDetailsId.Of(id), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "GET /api/tax-rule-details/{id} → 404 when id is malformed")]
    public async Task Get_ShouldReturn404_WhenIdMalformed()
    {
        const string bad = "not-a-guid";

        var res = await _client.GetAsync($"/api/tax-rule-details/{bad}");

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);

        _taxRuleDetailsRepoMock.Verify(r => r.GetByIdAsync(It.IsAny<TaxRuleDetailsId>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "GET /api/tax-rule-details/{id} → 200 for disabled TaxRuleDetail")]
    public async Task Get_ShouldReturn200_WhenTaxRuleDetailDisabled()
    {
        var id = Guid.NewGuid();
        var entity = Make(id, ApplicationRule.Fees, enabled: false);

        _taxRuleDetailsRepoMock.Setup(r => r.GetByIdAsync(TaxRuleDetailsId.Of(id), It.IsAny<CancellationToken>()))
             .ReturnsAsync(entity);

        var res = await _client.GetAsync($"/api/tax-rule-details/{id}");
        var dto = await res.Content.ReadFromJsonAsync<TaxRuleDetailDto>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.IsEnabled.Should().BeFalse();
        dto.AppliedOn.Should().Be("Fees");
    }

    [Fact(DisplayName = "GET /api/tax-rule-details/{id} → 200 with ApplicationRule.Fees")]
    public async Task Get_ShouldReturn200_WithFeesApplicationRule()
    {
        var id = Guid.NewGuid();
        var entity = Make(id, ApplicationRule.Fees);

        _taxRuleDetailsRepoMock.Setup(r => r.GetByIdAsync(TaxRuleDetailsId.Of(id), It.IsAny<CancellationToken>()))
             .ReturnsAsync(entity);

        var res = await _client.GetAsync($"/api/tax-rule-details/{id}");
        var dto = await res.Content.ReadFromJsonAsync<TaxRuleDetailDto>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.AppliedOn.Should().Be("Fees");
        dto.IsEnabled.Should().BeTrue();
    }

    [Fact(DisplayName = "GET /api/tax-rule-details/{id} → 200 with ApplicationRule.Amount")]
    public async Task Get_ShouldReturn200_WithAmountApplicationRule()
    {
        var id = Guid.NewGuid();
        var entity = Make(id, ApplicationRule.Amount);

        _taxRuleDetailsRepoMock.Setup(r => r.GetByIdAsync(TaxRuleDetailsId.Of(id), It.IsAny<CancellationToken>()))
             .ReturnsAsync(entity);

        var res = await _client.GetAsync($"/api/tax-rule-details/{id}");
        var dto = await res.Content.ReadFromJsonAsync<TaxRuleDetailDto>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.AppliedOn.Should().Be("Amount");
        dto.IsEnabled.Should().BeTrue();
    }
}