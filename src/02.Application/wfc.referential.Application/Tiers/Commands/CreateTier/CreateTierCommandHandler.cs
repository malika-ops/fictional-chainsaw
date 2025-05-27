using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.TierAggregate;
using wfc.referential.Domain.TierAggregate.Exceptions;

namespace wfc.referential.Application.Tiers.Commands.CreateTier;

public class CreateTierCommandHandler : ICommandHandler<CreateTierCommand, Result<Guid>>
{
    private readonly ITierRepository _repo;

    public CreateTierCommandHandler(ITierRepository repo) => _repo = repo;

    public async Task<Result<Guid>> Handle(CreateTierCommand cmd, CancellationToken ct)
    {
        var existingTier = await _repo.GetOneByConditionAsync(t => t.Name.ToLower().Equals(cmd.Name.ToLower()), ct);

        if (existingTier is not null)
            throw new TierNameAlreadyExistException($"Tier with name '{cmd.Name}' already exists.");

        var tierId = new TierId(Guid.NewGuid());

        var entity = Tier.Create(
            tierId,
            cmd.Name,
            cmd.Description);

        await _repo.AddAsync(entity, ct);

        await _repo.SaveChangesAsync(ct);

        return Result.Success(entity.Id!.Value);
    }
}
