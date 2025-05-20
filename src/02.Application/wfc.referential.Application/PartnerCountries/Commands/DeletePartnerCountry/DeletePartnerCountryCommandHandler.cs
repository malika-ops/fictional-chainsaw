using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;

namespace wfc.referential.Application.PartnerCountries.Commands.DeletePartnerCountry;

public class DeletePartnerCountryCommandHandler : ICommandHandler<DeletePartnerCountryCommand, Result<bool>>
{
    private readonly IPartnerCountryRepository _repo;

    public DeletePartnerCountryCommandHandler(IPartnerCountryRepository repo) => _repo = repo;

    public async Task<Result<bool>> Handle(DeletePartnerCountryCommand req, CancellationToken ct)
    {
        var entity = await _repo.GetByIdAsync(req.PartnerCountryId, ct);
        if (entity is null)
            throw new BusinessException($"PartnerCountry [{req.PartnerCountryId}] not found.");

        entity.Disable();                     
        await _repo.UpdateAsync(entity, ct);    

        return Result.Success(true);
    }
}