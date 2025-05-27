using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.TierAggregate;
using wfc.referential.Domain.TierAggregate.Exceptions;

namespace wfc.referential.Application.Tiers.Commands.PatchTier;

public class PatchTierCommandHandler : ICommandHandler<PatchTierCommand, Result<bool>>
{
    private readonly ITierRepository _tierRepo;
    public PatchTierCommandHandler(ITierRepository repo) => _tierRepo = repo;

    public async Task<Result<bool>> Handle(PatchTierCommand cmd, CancellationToken ct)
    {
        var tierId = TierId.Of(cmd.TierId);

        var tier = await _tierRepo.GetByIdAsync(tierId, ct) 
            ?? throw new ResourceNotFoundException($"Tier with id '{cmd.TierId}' not found.");

        if (!string.IsNullOrEmpty(cmd.Name))
        {
            var existingTier = await _tierRepo.GetOneByConditionAsync(t => t.Name.ToLower().Equals(cmd.Name.ToLower()), ct);
            if (existingTier is not null &&
                existingTier.Id != tier.Id)
                throw new TierNameAlreadyExistException($"Tier name '{cmd.Name}' already exists.");
        }
        
        tier.Patch(cmd.Name, cmd.Description, cmd.IsEnabled);             

        await _tierRepo.SaveChangesAsync(ct);

        return Result.Success(true);
    }
}
