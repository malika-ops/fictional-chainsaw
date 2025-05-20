using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.AgencyTierAggregate;
using wfc.referential.Domain.MonetaryZoneAggregate.Exceptions;

namespace wfc.referential.Application.AgencyTiers.Commands.DeleteAgencyTier;

public class DeleteAgencyTierCommandHandler : ICommandHandler<DeleteAgencyTierCommand, Result<bool>>
{
    private readonly IAgencyTierRepository _repo;

    public DeleteAgencyTierCommandHandler(IAgencyTierRepository repo) => _repo = repo;

    public async Task<Result<bool>> Handle(DeleteAgencyTierCommand req, CancellationToken ct)
    {
        var entity = await _repo.GetByIdAsync(AgencyTierId.Of(req.AgencyTierId), ct);
        if (entity is null)
            throw new InvalidDeletingException("AgencyTier not found.");

        entity.Disable();
        await _repo.UpdateAsync(entity, ct);

        return Result.Success(true);
    }
}