using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.AgencyAggregate;
using wfc.referential.Domain.AgencyTierAggregate;
using wfc.referential.Domain.AgencyTierAggregate.Exceptions;
using wfc.referential.Domain.TierAggregate;

namespace wfc.referential.Application.AgencyTiers.Commands.UpdateAgencyTier;

public class UpdateAgencyTierCommandHandler : ICommandHandler<UpdateAgencyTierCommand, Result<Guid>>
{
    private readonly IAgencyTierRepository _repo;

    public UpdateAgencyTierCommandHandler(IAgencyTierRepository repo) => _repo = repo;

    public async Task<Result<Guid>> Handle(UpdateAgencyTierCommand r, CancellationToken ct)
    {
        var agencyTier = await _repo.GetByIdAsync(new AgencyTierId(r.AgencyTierId), ct);
        if (agencyTier is null)
            throw new BusinessException("AgencyTier not found.");


        var duplicate = await _repo.GetByCodeAsync(r.Code, ct);
        if (duplicate is not null && duplicate.Id != agencyTier.Id)
            throw new DuplicateAgencyTierCodeException(r.Code);

        agencyTier.Update(
            new AgencyId(r.AgencyId),
            new TierId(r.TierId),
            r.Code,
            r.Password,
            r.IsEnabled);

        await _repo.UpdateAsync(agencyTier, ct);
        return Result.Success(agencyTier.Id.Value);
    }
}