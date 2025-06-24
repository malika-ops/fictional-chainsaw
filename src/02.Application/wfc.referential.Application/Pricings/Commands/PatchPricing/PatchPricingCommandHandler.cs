using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.AffiliateAggregate;
using wfc.referential.Domain.CorridorAggregate;
using wfc.referential.Domain.PricingAggregate;
using wfc.referential.Domain.PricingAggregate.Exceptions;
using wfc.referential.Domain.ServiceAggregate;

namespace wfc.referential.Application.Pricings.Commands.PatchPricing;

public class PatchPricingCommandHandler : ICommandHandler<PatchPricingCommand, Result<bool>>
{
    private readonly IPricingRepository _pricingRepo;
    private readonly IServiceRepository _serviceRepo;
    private readonly ICorridorRepository _corridorRepo;
    private readonly IAffiliateRepository _affiliateRepo;

    public PatchPricingCommandHandler(
        IPricingRepository pricingRepo,
        IServiceRepository serviceRepo,
        ICorridorRepository corridorRepo,
        IAffiliateRepository affiliateRepo)
    {
        _pricingRepo = pricingRepo;
        _serviceRepo = serviceRepo;
        _corridorRepo = corridorRepo;
        _affiliateRepo = affiliateRepo;
    }

    public async Task<Result<bool>> Handle(PatchPricingCommand cmd, CancellationToken ct)
    {
        var pricingId = PricingId.Of(cmd.PricingId);

        var pricing = await _pricingRepo.GetByIdAsync(pricingId, ct)
            ?? throw new ResourceNotFoundException($"Pricing '{pricingId.Value}' not found.");

        ServiceId? serviceId = null;
        CorridorId? corridorId = null;
        AffiliateId? affiliateId = null;

        if (cmd.ServiceId.HasValue)
        {
            serviceId = ServiceId.Of(cmd.ServiceId.Value);
            var service = await _serviceRepo.GetByIdAsync(serviceId, ct)
                ?? throw new ResourceNotFoundException($"Service '{serviceId.Value}' not found.");
        }

        if (cmd.CorridorId.HasValue)
        {
            corridorId = CorridorId.Of(cmd.CorridorId.Value);
            var corridor = await _corridorRepo.GetByIdAsync(corridorId, ct)
                ?? throw new ResourceNotFoundException($"Corridor '{corridorId.Value}' not found.");
        }

        if (cmd.AffiliateId.HasValue)
        {
            affiliateId = AffiliateId.Of(cmd.AffiliateId.Value);
            var affiliate = await _affiliateRepo.GetByIdAsync(affiliateId, ct)
                ?? throw new ResourceNotFoundException($"Affiliate '{affiliateId.Value}' not found.");
        }

        if (cmd.Code is not null && !cmd.Code.Equals(pricing.Code, StringComparison.OrdinalIgnoreCase))
        {
            var duplicate = await _pricingRepo.GetOneByConditionAsync(
                p => p.Code.ToLower() == cmd.Code.ToLower(), ct);

            if (duplicate is not null && duplicate.Id != pricing.Id)
                throw new DuplicatePricingCodeException(cmd.Code);
        }

        pricing.Patch(
            cmd.Code,
            cmd.Channel,
            cmd.MinimumAmount,
            cmd.MaximumAmount,
            cmd.FixedAmount,
            cmd.Rate,
            corridorId,
            serviceId,
            affiliateId,
            cmd.IsEnabled);

        await _pricingRepo.SaveChangesAsync(ct);
        return Result.Success(true);
    }
}
