using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.PricingAggregate;

namespace wfc.referential.Application.Pricings.Commands.DeletePricing;

public class DeletePricingCommandHandler : ICommandHandler<DeletePricingCommand, Result<bool>>
{
    private readonly IPricingRepository _pricingRepo;

    public DeletePricingCommandHandler(IPricingRepository pricingRepo)
    {
        _pricingRepo = pricingRepo;
    }

    public async Task<Result<bool>> Handle(DeletePricingCommand req, CancellationToken ct)
    {
        var pricingId = PricingId.Of(req.PricingId);

        var entity = await _pricingRepo.GetByIdAsync(pricingId, ct)
            ?? throw new ResourceNotFoundException($"Pricing {req.PricingId} not found.");

        entity.Disable();               

        await _pricingRepo.SaveChangesAsync(ct);
        return Result.Success(true);
    }
}
