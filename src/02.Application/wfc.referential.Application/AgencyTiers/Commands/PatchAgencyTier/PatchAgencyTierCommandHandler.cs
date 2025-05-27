using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.AgencyAggregate;
using wfc.referential.Domain.AgencyTierAggregate;
using wfc.referential.Domain.AgencyTierAggregate.Exceptions;
using wfc.referential.Domain.TierAggregate;

namespace wfc.referential.Application.AgencyTiers.Commands.PatchAgencyTier;

public class PatchAgencyTierCommandHandler : ICommandHandler<PatchAgencyTierCommand, Result<bool>>
{
    private readonly IAgencyTierRepository _agencyTierRepo;
    private readonly IAgencyRepository _agencyRepo;
    private readonly ITierRepository _tierRepo;


    public PatchAgencyTierCommandHandler(IAgencyTierRepository agencyTierRepo,
        IAgencyRepository agencyRepository,
        ITierRepository tierRepository)
    {
        _agencyTierRepo = agencyTierRepo;
        _agencyRepo = agencyRepository;
        _tierRepo = tierRepository;
    }

    public async Task<Result<bool>> Handle(PatchAgencyTierCommand cmd, CancellationToken ct)
    {
        var agencyTierId = AgencyTierId.Of(cmd.AgencyTierId);
        AgencyId agencyId;
        TierId tierId;

        var agencyTier = await _agencyTierRepo.GetByIdAsync(agencyTierId, ct)
            ?? throw new ResourceNotFoundException($"AgencyTier with id '{cmd.AgencyTierId}' not found.");

        if (cmd.TierId is not null)
        {
            tierId = TierId.Of((Guid)cmd.TierId);

            var tier = await _tierRepo.GetByIdAsync(tierId, ct)
                ?? throw new ResourceNotFoundException($"Tier with id '{cmd.TierId}' not found.");
        }
        else
        {
            tierId = agencyTier.TierId;
        }

        if (cmd.AgencyId is not null)
        {
            agencyId = AgencyId.Of((Guid)cmd.AgencyId);

            var agency = await _agencyRepo.GetByIdAsync(agencyId, ct)
                ?? throw new ResourceNotFoundException($"Agency with id '{cmd.AgencyId}' not found.");
        }
        else
        {
            agencyId = agencyTier.AgencyId;
        }


        // uniqueness check : the combination of (req.TierId, req.Code, req.AgencyId) must be unique
        if (!string.IsNullOrEmpty(cmd.Code) ||
            cmd.TierId is not null || cmd.AgencyId is not null) 
        {
            var newCode = string.IsNullOrEmpty(cmd.Code) ? agencyTier.Code : cmd.Code;

            var existingAgencyTier = await _agencyTierRepo.GetOneByConditionAsync(
           at => at.AgencyId == agencyId &&
           at.TierId == tierId &&
           at.Code.ToLower().Equals(newCode.ToLower()),
           ct);

            if (existingAgencyTier is not null && existingAgencyTier.Id!.Value != cmd.AgencyTierId)
                throw new DuplicateAgencyTierCodeException(cmd.Code);
        }

        agencyTier.Patch(
            agencyId,
            tierId,
            cmd.Code,
            cmd.Password,
            cmd.IsEnabled);

        await _agencyTierRepo.SaveChangesAsync(ct);

        return Result.Success(true);
    }
}