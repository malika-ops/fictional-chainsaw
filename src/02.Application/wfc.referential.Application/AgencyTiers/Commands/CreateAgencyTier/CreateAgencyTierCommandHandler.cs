using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.AgencyAggregate;
using wfc.referential.Domain.AgencyTierAggregate;
using wfc.referential.Domain.AgencyTierAggregate.Exceptions;
using wfc.referential.Domain.TierAggregate;


namespace wfc.referential.Application.AgencyTiers.Commands.CreateAgencyTier;

public class CreateAgencyTierCommandHandler : ICommandHandler<CreateAgencyTierCommand, Result<Guid>>
{
    private readonly IAgencyTierRepository _agencyTierRepo;
    private readonly IAgencyRepository _agencyRepo;
    private readonly ITierRepository _tierRepo;

    public CreateAgencyTierCommandHandler(IAgencyTierRepository agencyTierRepo, 
        IAgencyRepository agencyRepo, 
        ITierRepository tierRepo)
    {
        _agencyTierRepo = agencyTierRepo;
        _agencyRepo = agencyRepo;
        _tierRepo = tierRepo;
    }

    public async Task<Result<Guid>> Handle(CreateAgencyTierCommand req, CancellationToken ct)
    {
        var agencyTierId = AgencyTierId.Of(Guid.NewGuid());
        var agencyId = AgencyId.Of(req.AgencyId);
        var tierId = TierId.Of(req.TierId);

        var existingAgency = await _agencyRepo.GetByIdAsync(agencyId, ct) ??
            throw new ResourceNotFoundException($"Agency with Id '{req.AgencyId}' not found");

        var existingTier = await _tierRepo.GetByIdAsync(tierId, ct) ??
            throw new ResourceNotFoundException($"Tier with Id '{req.TierId}' not found");

        // the combination of tierId, agencyId and code should be unique
        var existingAgencyTier = await _agencyTierRepo.GetOneByConditionAsync(
            at => at.AgencyId == agencyId && 
            at.TierId == tierId && 
            at.Code.ToLower().Equals(req.Code.ToLower()),
            ct);

        // uniqueness check   // req.TierId, req.Code, req.AgencyId
        if (existingAgencyTier is not null)
            throw new DuplicateAgencyTierCodeException(req.Code);

        var entity = AgencyTier.Create(
            agencyTierId,
            agencyId,
            tierId,
            req.Code,
            req.Password);

        await _agencyTierRepo.AddAsync(entity, ct);

        await _agencyTierRepo.SaveChangesAsync(ct);

        return Result.Success(entity.Id!.Value);
    }
}