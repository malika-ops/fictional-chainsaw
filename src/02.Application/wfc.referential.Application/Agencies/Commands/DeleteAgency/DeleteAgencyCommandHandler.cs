using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.AgencyAggregate;

namespace wfc.referential.Application.Agencies.Commands.DeleteAgency;

public class DeleteAgencyCommandHandler : ICommandHandler<DeleteAgencyCommand, Result<bool>>
{
    private readonly IAgencyRepository _repo;

    public DeleteAgencyCommandHandler(IAgencyRepository repo) => _repo = repo;

    public async Task<Result<bool>> Handle(DeleteAgencyCommand cmd, CancellationToken ct)
    {
        var agency = await _repo.GetByIdAsync(AgencyId.Of(cmd.AgencyId), ct);
        if (agency is null)
            throw new BusinessException($"Agency [{cmd.AgencyId}] not found.");

        agency.Disable();
        await _repo.SaveChangesAsync(ct);
        return Result.Success(true);
    }
}