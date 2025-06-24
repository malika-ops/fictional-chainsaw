using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.AffiliateAggregate;
using wfc.referential.Domain.CorridorAggregate;
using wfc.referential.Domain.PricingAggregate;
using wfc.referential.Domain.PricingAggregate.Exceptions;
using wfc.referential.Domain.ServiceAggregate;

namespace wfc.referential.Application.Pricings.Commands.UpdatePricing;

public class UpdatePricingCommandHandler : ICommandHandler<UpdatePricingCommand, Result<bool>>
{
    private readonly IPricingRepository _pricingRepo;
    private readonly IServiceRepository _serviceRepo;
    private readonly ICorridorRepository _corridorRepo;
    private readonly IAffiliateRepository _affiliateRepo;

    public UpdatePricingCommandHandler(
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

    public async Task<Result<bool>> Handle(UpdatePricingCommand cmd, CancellationToken ct)
    {
        var pricingId = PricingId.Of(cmd.PricingId);
        var serviceId = ServiceId.Of(cmd.ServiceId);
        var corridorId = CorridorId.Of(cmd.CorridorId);
        var affiliateId = cmd.AffiliateId.HasValue ? AffiliateId.Of(cmd.AffiliateId.Value) : null;


        var currentPricing = await _pricingRepo.GetByIdAsync(pricingId, ct)
            ?? throw new ResourceNotFoundException($"Pricing '{pricingId.Value}' not found.");

        var service = await _serviceRepo.GetByIdAsync(serviceId, ct)
            ?? throw new ResourceNotFoundException($"Service '{serviceId.Value}' not found.");

        var corridor = await _corridorRepo.GetByIdAsync(corridorId, ct)
            ?? throw new ResourceNotFoundException($"Corridor '{corridorId.Value}' not found.");

        if (affiliateId is not null)
        {
            var affiliate = await _affiliateRepo.GetByIdAsync(affiliateId, ct)
                ?? throw new ResourceNotFoundException($"Affiliate '{affiliateId.Value}' not found.");
        }

        var sameCode = await _pricingRepo.GetOneByConditionAsync(
            p => p.Code.ToLower() == cmd.Code.ToLower(), ct);

        if (sameCode is not null && sameCode.Id!.Value != cmd.PricingId)
            throw new DuplicatePricingCodeException(cmd.Code);

        currentPricing.Update(
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
