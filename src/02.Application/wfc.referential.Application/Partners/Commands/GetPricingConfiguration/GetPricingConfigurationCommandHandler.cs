using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Application.Partners.Dtos;
using wfc.referential.Domain.AffiliateAggregate;
using wfc.referential.Domain.CorridorAggregate;
using wfc.referential.Domain.PartnerAggregate;
using wfc.referential.Domain.ServiceAggregate;

namespace wfc.referential.Application.Partners.Commands.GetPricingConfiguration;

public class GetPricingConfigurationCommandHandler : ICommandHandler<GetPricingConfigurationCommand, GetPricingConfigurationResponse>
{
    private readonly IContractRepository _contractRepo;
    private readonly IContractDetailsRepository _contractDetailsRepo;
    private readonly IPricingRepository _pricingRepo;
    private readonly ITaxRuleDetailRepository _taxRuleRepo;

    public GetPricingConfigurationCommandHandler(
        IContractRepository contractsRepo,
        IContractDetailsRepository contractDetailsRepository,
        IPricingRepository pricings,
        ITaxRuleDetailRepository taxRuleRepo)
    {
        _contractRepo = contractsRepo;
        _contractDetailsRepo = contractDetailsRepository;
        _pricingRepo = pricings;
        _taxRuleRepo = taxRuleRepo;
    }

    public async Task<GetPricingConfigurationResponse> Handle(
        GetPricingConfigurationCommand query,
        CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;


        var partnerId = PartnerId.Of(query.PartnerId);
        var affiliateId = AffiliateId.Of(query.AffiliateId);
        var corridorId = CorridorId.Of(query.CorridorId);
        var serviceId = ServiceId.Of(query.ServiceId);

        // 1) Find the active contract for this partner
        var contracts = await _contractRepo.GetByConditionAsync(
            c => c.PartnerId == partnerId
                 && c.IsEnabled
                 && c.StartDate <= now
                 && c.EndDate >= now,
            ct);

        var contract = contracts
            .OrderByDescending(c => c.CreatedAt)
            .FirstOrDefault()
            ?? throw new ResourceNotFoundException(
                   $"No active Contract found for Partner {query.PartnerId}");

        // 2) Find all enabled ContractDetails for that contract
        var details = await _contractDetailsRepo.GetByConditionAsync(
            cd => cd.ContractId == contract.Id && cd.IsEnabled,
            ct);

        if (!details.Any())
            throw new ResourceNotFoundException(
                $"No Pricing entries linked to Contract {contract.Id.Value}");

        var pricingIds = details
            .Select(cd => cd.PricingId)
            .Distinct()
            .ToList();

        // 3) From those, pick the one Pricing that matches channel/affiliate/corridor/service/amount
        var pricings = await _pricingRepo.GetByConditionAsync(
            p => pricingIds.Contains(p.Id)
                 && p.IsEnabled
                 && p.Channel == query.Channel
                 && p.AffiliateId == affiliateId
                 && p.CorridorId == corridorId
                 && p.ServiceId == serviceId
                 && p.MinimumAmount <= query.Amount
                 && p.MaximumAmount >= query.Amount,
            ct);

        var pricing = pricings
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefault()
            ?? throw new ResourceNotFoundException(
                   "No matching Pricing found for the provided parameters.");

        // 4) Load all TaxRuleDetails (with Tax nav) for this service/corridor
        var ruleDetails = await _taxRuleRepo.GetByConditionWithIncludesAsync(
            trd => trd.ServiceId == serviceId
                && trd.CorridorId == corridorId
                && trd.IsEnabled,
            ct,
            trd => trd.Tax);

        // 5) Project into the response DTO
        return new GetPricingConfigurationResponse
        {
            PartnerId = query.PartnerId,
            Contract = new()
            {
                ContractId = contract.Id!.Value,
                Code = contract.Code,
                StartDate = contract.StartDate,
                EndDate = contract.EndDate
            },
            Pricing = new()
            {
                PricingId = pricing.Id!.Value,
                FixedAmount = pricing.FixedAmount,
                Rate = pricing.Rate,
                MinimumAmount = pricing.MinimumAmount,
                MaximumAmount = pricing.MaximumAmount,
                Channel = pricing.Channel,
                AffiliateId = pricing.AffiliateId!.Value,
                CorridorId = pricing.CorridorId.Value,
                ServiceId = pricing.ServiceId.Value
            },
            Taxes = ruleDetails.Select(trd => 
            {
                var tax = trd.Tax;
                var rate = tax.Rate.HasValue ? (decimal)tax.Rate.Value : 0m;
                var fixedAmt = tax.FixedAmount.HasValue ? (decimal)tax.FixedAmount.Value : 0m;
                return new GetPricingConfigurationResponse.TaxDto
                {
                    TaxId = tax.Id!.Value,
                    TaxCode = tax.Code,
                    AppliedOn = trd.AppliedOn,
                    Rate = rate,
                    FixedAmount = fixedAmt
                };
            }).ToList()
        };
    }

}