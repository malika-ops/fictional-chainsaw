using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.TierAggregate;

namespace wfc.referential.Application.Tiers.Commands.DeleteTier;

public class DeleteTierCommandHandler : ICommandHandler<DeleteTierCommand, Result<bool>>
{
    private readonly ITierRepository _repo;
    public DeleteTierCommandHandler(ITierRepository repo) => _repo = repo;

    public async Task<Result<bool>> Handle(DeleteTierCommand cmd, CancellationToken ct)
    {
        var tier = await _repo.GetByIdAsync(new TierId(cmd.TierId), ct);
        if (tier is null)
            throw new BusinessException("Tier not found.");

        tier.Disable();                     
        await _repo.UpdateAsync(tier, ct);    

        return Result.Success(true);
    }
}