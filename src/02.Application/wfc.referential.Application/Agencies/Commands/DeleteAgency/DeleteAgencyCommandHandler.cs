using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.AgencyAggregate;

namespace wfc.referential.Application.Agencies.Commands.DeleteAgency;

public class DeleteAgencyCommandHandler : ICommandHandler<DeleteAgencyCommand, Result<bool>>
{
    private readonly IAgencyRepository _agencyRepo;
    private readonly ICorridorRepository _corridorRepo;

    public DeleteAgencyCommandHandler(IAgencyRepository agencyRepo, 
        ICorridorRepository corridorRepo)
    {
        _agencyRepo = agencyRepo;
        _corridorRepo = corridorRepo;
    }

    public async Task<Result<bool>> Handle(DeleteAgencyCommand command, CancellationToken ct)
    {
        var agencyId = AgencyId.Of(command.AgencyId);

        var agency = await _agencyRepo.GetByIdAsync(agencyId, ct) 
            ?? throw new ResourceNotFoundException($"Agency [{command.AgencyId}] not found.");


        agency.Disable();
        await _agencyRepo.SaveChangesAsync(ct);
        return Result.Success(true);
    }
}