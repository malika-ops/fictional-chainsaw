using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using Mapster;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.TierAggregate;

namespace wfc.referential.Application.Tiers.Commands.PatchTier;

public class PatchTierCommandHandler : ICommandHandler<PatchTierCommand, Result<Guid>>
{
    private readonly ITierRepository _repo;
    public PatchTierCommandHandler(ITierRepository repo) => _repo = repo;

    public async Task<Result<Guid>> Handle(PatchTierCommand cmd, CancellationToken ct)
    {
        var tier = await _repo.GetByIdAsync(new TierId(cmd.TierId), ct);
        if (tier is null)
            throw new BusinessException("Tier not found.");


        if (!string.IsNullOrEmpty(cmd.Name))
        {
            var dup = await _repo.GetByNameAsync(cmd.Name, ct);
            if (dup is not null && dup.Id != tier.Id)
                throw new BusinessException($"Tier '{cmd.Name}' already exists.");
        }

        // Map non-null props onto entity
        cmd.Adapt(tier);          
        tier.Patch();             

        await _repo.UpdateAsync(tier, ct);
        return Result.Success(tier.Id.Value);
    }
}
