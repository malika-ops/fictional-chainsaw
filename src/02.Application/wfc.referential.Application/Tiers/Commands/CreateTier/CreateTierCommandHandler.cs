using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.TierAggregate;

namespace wfc.referential.Application.Tiers.Commands.CreateTier;

public class CreateTierCommandHandler : ICommandHandler<CreateTierCommand, Result<Guid>>
{
    private readonly ITierRepository _repo;

    public CreateTierCommandHandler(ITierRepository repo) => _repo = repo;

    public async Task<Result<Guid>> Handle(CreateTierCommand cmd,
                                           CancellationToken ct)
    {
        if (await _repo.GetByNameAsync(cmd.Name, ct) is not null)
            throw new BusinessException($"Tier '{cmd.Name}' already exists.");

        var entity = Tier.Create(
            new TierId(Guid.NewGuid()),
            cmd.Name,
            cmd.Description,
            cmd.IsEnabled);

        await _repo.AddAsync(entity, ct);
        return Result.Success(entity.Id!.Value);
    }
}
