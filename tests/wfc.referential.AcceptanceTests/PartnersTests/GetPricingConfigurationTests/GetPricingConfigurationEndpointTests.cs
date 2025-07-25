using AutoFixture;
using FluentAssertions;
using Moq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using wfc.referential.Application.Partners.Dtos;
using wfc.referential.Domain.AffiliateAggregate;
using wfc.referential.Domain.ContractAggregate;
using wfc.referential.Domain.ContractDetailsAggregate;
using wfc.referential.Domain.CorridorAggregate;
using wfc.referential.Domain.PartnerAggregate;
using wfc.referential.Domain.PricingAggregate;
using wfc.referential.Domain.ServiceAggregate;
using wfc.referential.Domain.TaxAggregate;
using wfc.referential.Domain.TaxRuleDetailAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.PartnersTests.GetPricingConfigurationTests;

public class GetPricingConfigurationEndpointTests(TestWebApplicationFactory factory)
    : BaseAcceptanceTests(factory)
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private static (
        Contract contract,
        ContractDetails cDetails,
        Pricing pricing,
        List<TaxRuleDetail> taxDetails
    ) BuildDomain(GetPricingConfigurationRequest req)
    {
        var contract = Contract.Create(
            ContractId.Of(Guid.NewGuid()),
            "CON‑001",
            PartnerId.Of(req.PartnerId),
            DateTime.UtcNow.AddDays(-5),
            DateTime.UtcNow.AddDays(+5));

        var pricing = Pricing.Create(
            PricingId.Of(Guid.NewGuid()),
            "PR‑001",
            req.Channel,
            minimumAmount: 1m,
            maximumAmount: 5_000m,
            fixedAmount: 10m,
            rate: 0.02m,
            CorridorId.Of(req.CorridorId),
            ServiceId.Of(req.ServiceId),
            AffiliateId.Of(req.AffiliateId));

        var cDetails = ContractDetails.Create(
            ContractDetailsId.Of(Guid.NewGuid()),
            contract.Id,
            pricing.Id);

        var tax = Tax.Create(
            TaxId.Of(Guid.NewGuid()),
            code: "VAT",
            codeEn: "VAT",
            codeAr: "ض.ق.م",
            description: "Value‑Added Tax",
            fixedAmount: 2,
            rate: 0.10);

        var trd = TaxRuleDetail.Create(
            TaxRuleDetailsId.Of(Guid.NewGuid()),
            CorridorId.Of(req.CorridorId),
            tax.Id,
            ServiceId.Of(req.ServiceId),
            ApplicationRule.Amount);

        typeof(TaxRuleDetail).GetProperty(nameof(TaxRuleDetail.Tax))!
                             .SetValue(trd, tax);

        return (contract, cDetails, pricing, new() { trd });
    }


    [Fact(DisplayName = "POST /api/partners/pricing-configuration/search → 200 with config")]
    public async Task Post_ShouldReturn200_WithConfig()
    {
        //  Arrange
        var req = _fixture.Build<GetPricingConfigurationRequest>()
                          .With(r => r.Amount, 100m)
                          .With(r => r.Channel, "Web")
                          .Create();

        var (contract, cDetails, pricing, taxes) = BuildDomain(req);

        _contractRepoMock
            .Setup(r => r.GetByConditionAsync(
                      It.IsAny<Expression<Func<Contract, bool>>>(),
                      It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Contract> { contract });

        _contractDetailsRepoMock
            .Setup(r => r.GetByConditionAsync(
                      It.IsAny<Expression<Func<ContractDetails, bool>>>(),
                      It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ContractDetails> { cDetails });

        _pricingRepoMock
            .Setup(r => r.GetByConditionAsync(
                      It.IsAny<Expression<Func<Pricing, bool>>>(),
                      It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Pricing> { pricing });

        _taxRuleDetailsRepoMock
            .Setup(r => r.GetByConditionWithIncludesAsync(
                      It.IsAny<Expression<Func<TaxRuleDetail, bool>>>(),
                      It.IsAny<CancellationToken>(),
                      It.IsAny<Expression<Func<TaxRuleDetail, object>>[]>()))
            .ReturnsAsync(taxes);

        //  Act
        var resp = await _client.PostAsJsonAsync("/api/partners/pricing-configuration/search", req);
        var dto = await resp.Content.ReadFromJsonAsync<GetPricingConfigurationResponse>(JsonOpts);

        //  Assert 
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        dto!.PartnerId.Should().Be(req.PartnerId);
        dto.Contract.ContractId.Should().Be(contract.Id.Value);
        dto.Pricing.PricingId.Should().Be(pricing.Id.Value);
        dto.Taxes.Should().HaveCount(1);
        dto.Taxes[0].TaxCode.Should().Be("VAT");

        _contractRepoMock.Verify(r => r.GetByConditionAsync(
                                   It.IsAny<Expression<Func<Contract, bool>>>(),
                                   It.IsAny<CancellationToken>()), Times.Once);

        _contractDetailsRepoMock.Verify(r => r.GetByConditionAsync(
                                          It.IsAny<Expression<Func<ContractDetails, bool>>>(),
                                          It.IsAny<CancellationToken>()), Times.Once);

        _pricingRepoMock.Verify(r => r.GetByConditionAsync(
                                   It.IsAny<Expression<Func<Pricing, bool>>>(),
                                   It.IsAny<CancellationToken>()), Times.Once);

        _taxRuleDetailsRepoMock.Verify(r => r.GetByConditionWithIncludesAsync(
                                         It.IsAny<Expression<Func<TaxRuleDetail, bool>>>(),
                                         It.IsAny<CancellationToken>(),
                                         It.IsAny<Expression<Func<TaxRuleDetail, object>>[]>()), Times.Once);
    }



    [Fact(DisplayName = "POST /api/partners/pricing-configuration/search → 404 when no active contract")]
    public async Task Post_ShouldReturn404_WhenNoContract()
    {
        var req = _fixture.Create<GetPricingConfigurationRequest>();

        _contractRepoMock.Setup(r => r.GetByConditionAsync(
                                   It.IsAny<Expression<Func<Contract, bool>>>(),
                                   It.IsAny<CancellationToken>()))
                         .ReturnsAsync(new List<Contract>());

        var resp = await _client.PostAsJsonAsync("/api/partners/pricing-configuration/search", req);

        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);

        _contractDetailsRepoMock.VerifyNoOtherCalls();
        _pricingRepoMock.VerifyNoOtherCalls();
        _taxRuleDetailsRepoMock.VerifyNoOtherCalls();
    }

    [Fact(DisplayName = "POST /api/partners/pricing-configuration/search → 404 when pricing mismatch")]
    public async Task Post_ShouldReturn404_WhenPricingMissing()
    {
        var req = _fixture.Create<GetPricingConfigurationRequest>();

        var contract = Contract.Create(
            ContractId.Of(Guid.NewGuid()),
            "C‑X",
            PartnerId.Of(req.PartnerId),
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow.AddDays(+1));

        _contractRepoMock.Setup(r => r.GetByConditionAsync(
                                   It.IsAny<Expression<Func<Contract, bool>>>(),
                                   It.IsAny<CancellationToken>()))
                         .ReturnsAsync(new List<Contract> { contract });

        var cd = ContractDetails.Create(
            ContractDetailsId.Of(Guid.NewGuid()),
            contract.Id,
            PricingId.Of(Guid.NewGuid()));

        _contractDetailsRepoMock.Setup(r => r.GetByConditionAsync(
                                         It.IsAny<Expression<Func<ContractDetails, bool>>>(),
                                         It.IsAny<CancellationToken>()))
                                .ReturnsAsync(new List<ContractDetails> { cd });

        _pricingRepoMock.Setup(r => r.GetByConditionAsync(
                                  It.IsAny<Expression<Func<Pricing, bool>>>(),
                                  It.IsAny<CancellationToken>()))
                        .ReturnsAsync(new List<Pricing>());

        var resp = await _client.PostAsJsonAsync("/api/partners/pricing-configuration/search", req);

        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }


    [Fact(DisplayName = "POST /api/partners/pricing-configuration/search → 404 when contract inactive")]
    public async Task Post_ShouldReturn404_WhenContractInactive()
    {
        var req = _fixture.Create<GetPricingConfigurationRequest>();

        var inactive = Contract.Create(
            ContractId.Of(Guid.NewGuid()),
            "OLD",
            PartnerId.Of(req.PartnerId),
            DateTime.UtcNow.AddDays(-10),
            DateTime.UtcNow.AddDays(-5));   

        _contractRepoMock.Setup(r => r.GetByConditionAsync(
                                   It.IsAny<Expression<Func<Contract, bool>>>(),
                                   It.IsAny<CancellationToken>()))
                         .ReturnsAsync(new List<Contract> { inactive });

        var resp = await _client.PostAsJsonAsync("/api/partners/pricing-configuration/search", req);

        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }


    [Fact(DisplayName = "POST /api/partners/pricing-configuration/search → 200 with zero taxes")]
    public async Task Post_ShouldReturn200_WhenNoTaxes()
    {
        var req = _fixture.Create<GetPricingConfigurationRequest>();
        var (contract, cd, pricing, _) = BuildDomain(req);

        _contractRepoMock.Setup(r => r.GetByConditionAsync(It.IsAny<Expression<Func<Contract, bool>>>(),
                                                           It.IsAny<CancellationToken>()))
                         .ReturnsAsync(new List<Contract> { contract });

        _contractDetailsRepoMock.Setup(r => r.GetByConditionAsync(It.IsAny<Expression<Func<ContractDetails, bool>>>(),
                                                                  It.IsAny<CancellationToken>()))
                                .ReturnsAsync(new List<ContractDetails> { cd });

        _pricingRepoMock.Setup(r => r.GetByConditionAsync(It.IsAny<Expression<Func<Pricing, bool>>>(),
                                                          It.IsAny<CancellationToken>()))
                        .ReturnsAsync(new List<Pricing> { pricing });

        _taxRuleDetailsRepoMock.Setup(r =>
            r.GetByConditionWithIncludesAsync(It.IsAny<Expression<Func<TaxRuleDetail, bool>>>(),
                                              It.IsAny<CancellationToken>(),
                                              It.IsAny<Expression<Func<TaxRuleDetail, object>>[]>()))
                               .ReturnsAsync(new List<TaxRuleDetail>());   

        var resp = await _client.PostAsJsonAsync("/api/partners/pricing-configuration/search", req);
        var dto = await resp.Content.ReadFromJsonAsync<GetPricingConfigurationResponse>(JsonOpts);

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Taxes.Should().BeEmpty();
    }


    [Fact(DisplayName = "POST /api/partners/pricing-configuration/search → 404 when contract disabled")]
    public async Task Post_ShouldReturn404_WhenContractDisabled()
    {
        var req = _fixture.Create<GetPricingConfigurationRequest>();

        var disabledContract = Contract.Create(
            ContractId.Of(Guid.NewGuid()), "DIS",
            PartnerId.Of(req.PartnerId),
            DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(+1));

        typeof(Contract).GetProperty("IsEnabled")!
                        .SetValue(disabledContract, false);

        _contractRepoMock.Setup(r => r.GetByConditionAsync(It.IsAny<Expression<Func<Contract, bool>>>(),
                                                           It.IsAny<CancellationToken>()))
                         .ReturnsAsync(new List<Contract> { disabledContract });

        var resp = await _client.PostAsJsonAsync("/api/partners/pricing-configuration/search", req);
        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }


    [Fact(DisplayName = "POST /api/partners/pricing-configuration/search → 404 when contract‑details disabled")]
    public async Task Post_ShouldReturn404_WhenContractDetailsDisabled()
    {
        var req = _fixture.Create<GetPricingConfigurationRequest>();
        var (contract, cd, pricing, _) = BuildDomain(req);

        typeof(ContractDetails).GetProperty("IsEnabled")!
                               .SetValue(cd, false);   

        _contractRepoMock.Setup(r => r.GetByConditionAsync(It.IsAny<Expression<Func<Contract, bool>>>(),
                                                           It.IsAny<CancellationToken>()))
                         .ReturnsAsync(new List<Contract> { contract });

        _contractDetailsRepoMock.Setup(r => r.GetByConditionAsync(It.IsAny<Expression<Func<ContractDetails, bool>>>(),
                                                                  It.IsAny<CancellationToken>()))
                                .ReturnsAsync(new List<ContractDetails> { cd });

        var resp = await _client.PostAsJsonAsync("/api/partners/pricing-configuration/search", req);
        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "POST /api/partners/pricing-configuration/search → 400 when PartnerId empty")]
    public async Task Post_ShouldReturn400_WhenPartnerIdEmpty()
    {
        var req = _fixture.Build<GetPricingConfigurationRequest>()
                          .With(r => r.PartnerId, Guid.Empty)  
                          .Create();

        var resp = await _client.PostAsJsonAsync("/api/partners/pricing-configuration/search", req);

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var txt = await resp.Content.ReadAsStringAsync();
        txt.Should().Contain("PartnerId is required.");
    }

    [Fact(DisplayName = "POST /api/partners/pricing-configuration/search → 400 when Channel length > 50")]
    public async Task Post_ShouldReturn400_WhenChannelTooLong()
    {
        var longChannel = new string('X', 51);
        var req = _fixture.Build<GetPricingConfigurationRequest>()
                          .With(r => r.Channel, longChannel)
                          .Create();

        var resp = await _client.PostAsJsonAsync("/api/partners/pricing-configuration/search", req);

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await resp.Content.ReadAsStringAsync()).Should().Contain("Channel must not exceed 50");
    }

    [Fact(DisplayName = "POST /api/partners/pricing-configuration/search → 400 when amount is zero")]
    public async Task Post_ShouldReturn400_WhenAmountZero()
    {
        var req = _fixture.Build<GetPricingConfigurationRequest>()
                          .With(r => r.Amount, 0m)
                          .Create();

        var resp = await _client.PostAsJsonAsync("/api/partners/pricing-configuration/search", req);

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        _contractRepoMock.VerifyNoOtherCalls();
    }
}