using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.PartnerAggregate;
using wfc.referential.Domain.PartnerCountryAggregate.Exceptions;

namespace wfc.referential.Application.PartnerCountries.Commands.UpdatePartnerCountry;

public class UpdatePartnerCountryCommandHandler : ICommandHandler<UpdatePartnerCountryCommand, Result<Guid>>
{
    private readonly IPartnerCountryRepository _repo;

    public UpdatePartnerCountryCommandHandler(IPartnerCountryRepository repo) => _repo = repo;

    public async Task<Result<Guid>> Handle(UpdatePartnerCountryCommand req, CancellationToken ct)
    {
        var entity = await _repo.GetByIdAsync(req.PartnerCountryId, ct)
                    ?? throw new BusinessException("PartnerCountry not found");

        var duplicate = await _repo.GetByPartnerAndCountryAsync(req.PartnerId, req.CountryId, ct);
        if (duplicate is not null && duplicate.Id != entity.Id)
            throw new PartnerCountryAlreadyExistException(req.PartnerId, req.CountryId);

        entity.Update(new PartnerId(req.PartnerId),
                      new CountryId(req.CountryId),
                      req.IsEnabled);

        await _repo.UpdateAsync(entity, ct);
        return Result.Success(entity.Id.Value);
    }
}
