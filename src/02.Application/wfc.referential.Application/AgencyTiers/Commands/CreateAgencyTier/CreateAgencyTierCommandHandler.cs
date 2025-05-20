using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.AgencyAggregate;
using wfc.referential.Domain.AgencyTierAggregate;
using wfc.referential.Domain.AgencyTierAggregate.Exceptions;
using wfc.referential.Domain.TierAggregate;


namespace wfc.referential.Application.AgencyTiers.Commands.CreateAgencyTier;

public class CreateAgencyTierCommandHandler : ICommandHandler<CreateAgencyTierCommand, Result<Guid>>
{
    private readonly IAgencyTierRepository _repo;

    public CreateAgencyTierCommandHandler(IAgencyTierRepository repo) => _repo = repo;

    public async Task<Result<Guid>> Handle(CreateAgencyTierCommand req, CancellationToken ct)
    {
        // uniqueness check
        if (await _repo.ExistsAsync(req.AgencyId, req.TierId, req.Code, ct))
            throw new DuplicateAgencyTierCodeException(req.Code);

        var entity = AgencyTier.Create(
            id: AgencyTierId.Of(Guid.NewGuid()),
            agencyId: new AgencyId(req.AgencyId),
            tierId: new TierId(req.TierId),
            code: req.Code,
            password: req.Password ?? string.Empty,
            isEnabled: req.IsEnabled);

        await _repo.AddAsync(entity, ct);
        return Result.Success(entity.Id.Value);
    }
}