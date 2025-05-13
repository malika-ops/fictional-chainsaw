using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.TierAggregate;

namespace wfc.referential.Application.Tiers.Commands.UpdateTier;

public class UpdateTierCommandHandler : ICommandHandler<UpdateTierCommand, Result<Guid>>
{
    private readonly ITierRepository _repo;
    public UpdateTierCommandHandler(ITierRepository repo) => _repo = repo;

    public async Task<Result<Guid>> Handle(UpdateTierCommand cmd, CancellationToken ct)
    {
        var tier = await _repo.GetByIdAsync(new TierId(cmd.TierId), ct);
        if (tier is null)
            throw new BusinessException("Tier not found.");

        var nameDuplicate = await _repo.GetByNameAsync(cmd.Name, ct);
        if (nameDuplicate is not null &&
            nameDuplicate.Id != tier.Id)
            throw new BusinessException($"Tier '{cmd.Name}' already exists.");

        tier.Update(cmd.Name, cmd.Description, cmd.IsEnabled);

        await _repo.UpdateAsync(tier, ct);
        return Result.Success(tier.Id.Value);
    }
}