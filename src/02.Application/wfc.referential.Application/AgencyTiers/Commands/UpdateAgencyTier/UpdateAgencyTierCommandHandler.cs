using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.AgencyAggregate;
using wfc.referential.Domain.AgencyTierAggregate;
using wfc.referential.Domain.AgencyTierAggregate.Exceptions;
using wfc.referential.Domain.TierAggregate;

namespace wfc.referential.Application.AgencyTiers.Commands.UpdateAgencyTier;

public class UpdateAgencyTierCommandHandler : ICommandHandler<UpdateAgencyTierCommand, Result<bool>>
{
    private readonly IAgencyTierRepository _agencyTierRepo;
    private readonly IAgencyRepository _agencyRepo;
    private readonly ITierRepository _tierRepo;


    public UpdateAgencyTierCommandHandler(IAgencyTierRepository agencyTierRepo, 
        IAgencyRepository agencyRepository, 
        ITierRepository tierRepository)
    {
        _agencyTierRepo = agencyTierRepo;
        _agencyRepo = agencyRepository;
        _tierRepo = tierRepository;
    }

    public async Task<Result<bool>> Handle(UpdateAgencyTierCommand cmd, CancellationToken ct)
    {

        var agencyTierId = AgencyTierId.Of(cmd.AgencyTierId);
        var agencyId = AgencyId.Of(cmd.AgencyId);
        var tierId = TierId.Of(cmd.TierId);

        var agencyTier = await _agencyTierRepo.GetByIdAsync(agencyTierId, ct) 
            ?? throw new ResourceNotFoundException($"AgencyTier with id '{cmd.AgencyTierId}' not found.");

        var agency = await _agencyRepo.GetByIdAsync(agencyId, ct)
            ?? throw new ResourceNotFoundException($"Agency with id '{cmd.AgencyId}' not found.");

        var tier = await _tierRepo.GetByIdAsync(tierId, ct)
            ?? throw new ResourceNotFoundException($"Tier with id '{cmd.TierId}' not found.");

        var existingAgencyTier = await _agencyTierRepo.GetOneByConditionAsync(
           at => at.AgencyId == agencyId &&
           at.TierId == tierId &&
           at.Code.ToLower().Equals(cmd.Code.ToLower()),
           ct);

        // uniqueness check : the combination of (req.TierId, req.Code, req.AgencyId) must be unique
        if (existingAgencyTier is not null && existingAgencyTier.Id!.Value != cmd.AgencyTierId)
            throw new DuplicateAgencyTierCodeException(cmd.Code);


        agencyTier.Update(
            agencyId,
            tierId,
            cmd.Code,
            cmd.Password,
            cmd.IsEnabled);

        await _agencyTierRepo.SaveChangesAsync(ct);

        return Result.Success(true);
    }
}