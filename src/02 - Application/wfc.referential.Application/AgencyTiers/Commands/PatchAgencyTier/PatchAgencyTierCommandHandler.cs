using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using Mapster;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.AgencyTierAggregate;
using wfc.referential.Domain.AgencyTierAggregate.Exceptions;

namespace wfc.referential.Application.AgencyTiers.Commands.PatchAgencyTier;

public class PatchAgencyTierCommandHandler : ICommandHandler<PatchAgencyTierCommand, Result<Guid>>
{
    private readonly IAgencyTierRepository _repo;

    public PatchAgencyTierCommandHandler(IAgencyTierRepository repo) => _repo = repo;

    public async Task<Result<Guid>> Handle(PatchAgencyTierCommand r, CancellationToken ct)
    {

        var entity = await _repo.GetByIdAsync(AgencyTierId.Of(r.AgencyTierId), ct);
        if (entity is null)
            throw new BusinessException("AgencyTier not found.");


        if (!string.IsNullOrEmpty(r.Code))
        {
            var dup = await _repo.GetByCodeAsync(r.Code, ct);
            if (dup is not null && dup.Id != entity.Id)
                throw new DuplicateAgencyTierCodeException(r.Code);
        }


        r.Adapt(entity);


        entity.Patch();
        await _repo.UpdateAsync(entity, ct);

        return Result.Success(entity.Id.Value);
    }
}