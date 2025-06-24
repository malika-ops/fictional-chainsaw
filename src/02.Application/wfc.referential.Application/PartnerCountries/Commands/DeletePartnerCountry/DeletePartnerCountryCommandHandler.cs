using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.PartnerCountryAggregate;

namespace wfc.referential.Application.PartnerCountries.Commands.DeletePartnerCountry;

public class DeletePartnerCountryCommandHandler : ICommandHandler<DeletePartnerCountryCommand, Result<bool>>
{
    private readonly IPartnerCountryRepository _repo;

    public DeletePartnerCountryCommandHandler(IPartnerCountryRepository repo)
    {
        _repo = repo;
    }

    public async Task<Result<bool>> Handle(DeletePartnerCountryCommand req, CancellationToken ct)
    {
        var partnerCountryId = PartnerCountryId.Of(req.PartnerCountryId);

        var entity = await _repo.GetByIdAsync(partnerCountryId, ct) 
            ?? throw new ResourceNotFoundException($"PartnerCountry [{req.PartnerCountryId}] not found.");

        entity.Disable();     
        
        await _repo.SaveChangesAsync(ct);

        return Result.Success(true);
    }
}