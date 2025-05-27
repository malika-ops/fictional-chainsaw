using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.AgencyTierAggregate;

namespace wfc.referential.Application.AgencyTiers.Commands.DeleteAgencyTier;

public class DeleteAgencyTierCommandHandler : ICommandHandler<DeleteAgencyTierCommand, Result<bool>>
{
    private readonly IAgencyTierRepository _agencyTierRepo;

    public DeleteAgencyTierCommandHandler(IAgencyTierRepository agencyTier)
    {
        _agencyTierRepo = agencyTier;
    }

    public async Task<Result<bool>> Handle(DeleteAgencyTierCommand req, CancellationToken ct)
    {
        var agencyTier = AgencyTierId.Of(req.AgencyTierId);

        var entity = await _agencyTierRepo.GetByIdAsync(agencyTier, ct) 
            ?? throw new ResourceNotFoundException($"AgencyTier {req.AgencyTierId} not found.");

        entity.Disable();

        await _agencyTierRepo.SaveChangesAsync(ct);

        return Result.Success(true);
    }
}