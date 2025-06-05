using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.AffiliateAggregate;
using wfc.referential.Domain.CorridorAggregate;
using wfc.referential.Domain.PricingAggregate;
using wfc.referential.Domain.PricingAggregate.Exceptions;
using wfc.referential.Domain.ServiceAggregate;

namespace wfc.referential.Application.Pricings.Commands.CreatePricing;

public class CreatePricingCommandHandler : ICommandHandler<CreatePricingCommand, Result<Guid>>
{
    private readonly IPricingRepository _pricingRepo;
    private readonly IServiceRepository _serviceRepo;
    private readonly ICorridorRepository _corridorRepo;
    private readonly IAffiliateRepository _affiliateRepo;

    public CreatePricingCommandHandler(
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

    public async Task<Result<Guid>> Handle(CreatePricingCommand req, CancellationToken ct)
    {

        var corridorId = CorridorId.Of(req.CorridorId);
        var serviceId = ServiceId.Of(req.ServiceId);
        var affiliateId = req.AffiliateId is not null ? AffiliateId.Of((Guid)req.AffiliateId) : null;

        // foreign-key existence checks

        if (affiliateId is not null)
        {
            var affiliate = await _affiliateRepo.GetByIdAsync(affiliateId, ct)
                ?? throw new ResourceNotFoundException($"Affiliate '{affiliateId}' not found.");
        }

        var corridor = await _corridorRepo.GetByIdAsync(corridorId, ct)
            ?? throw new ResourceNotFoundException($"Corridor '{corridorId}' not found.");


        // uniqueness check (by Code)
        var pricingWithSameCode = await _pricingRepo.GetOneByConditionAsync(pr => pr.Code.ToLower().Equals(req.Code.ToLower()), ct);
        
        if(pricingWithSameCode is not null)
            throw new DuplicatePricingCodeException(req.Code);

        var pricingId = PricingId.Of(Guid.NewGuid());

        var pricing = Pricing.Create(
            pricingId,
            req.Code,
            req.Channel,
            req.MinimumAmount,
            req.MaximumAmount,
            req.FixedAmount,
            req.Rate,
            corridorId,
            serviceId,
            affiliateId);

        await _pricingRepo.AddAsync(pricing, ct);
        await _pricingRepo.SaveChangesAsync(ct);

        return Result.Success(pricing.Id!.Value);
    }
}
