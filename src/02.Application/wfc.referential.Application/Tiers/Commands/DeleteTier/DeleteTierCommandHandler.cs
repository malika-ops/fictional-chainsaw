using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.TierAggregate;

namespace wfc.referential.Application.Tiers.Commands.DeleteTier;

public class DeleteTierCommandHandler : ICommandHandler<DeleteTierCommand, Result<bool>>
{
    private readonly ITierRepository _tierRepo;
    private readonly IAgencyTierRepository _agencyTierRepo;

    public DeleteTierCommandHandler(ITierRepository tierRepo, IAgencyTierRepository agencyTierRepo)
    {
        _tierRepo = tierRepo;
        _agencyTierRepo = agencyTierRepo;
    }

    public async Task<Result<bool>> Handle(DeleteTierCommand cmd, CancellationToken ct)
    {
        var tierId = TierId.Of(cmd.TierId);

        var tier = await _tierRepo.GetByIdAsync(tierId, ct) 
            ?? throw new ResourceNotFoundException($"Tier with id '{cmd.TierId}' not found.");

        var linkedTier = await _agencyTierRepo.GetOneByConditionAsync(
            at => at.TierId == tierId && at.IsEnabled == true, ct);

        if (linkedTier is not null) 
        { 
            throw new BusinessException($"Tier with id '{cmd.TierId}' is linked to an existing agency");
        }

        tier.Disable();          
        
        await _tierRepo.SaveChangesAsync(ct);    

        return Result.Success(true);
    }
}