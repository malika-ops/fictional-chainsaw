using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.PartnerAggregate;
using wfc.referential.Domain.PartnerCountryAggregate;
using wfc.referential.Domain.PartnerCountryAggregate.Exceptions;

namespace wfc.referential.Application.PartnerCountries.Commands.CreatePartnerCountry;

public class CreatePartnerCountryCommandHandler : ICommandHandler<CreatePartnerCountryCommand, Result<Guid>>
{
    private readonly IPartnerCountryRepository _repo;

    public CreatePartnerCountryCommandHandler(IPartnerCountryRepository repo)
        => _repo = repo;

    public async Task<Result<Guid>> Handle(CreatePartnerCountryCommand req,
                                           CancellationToken ct)
    {
        var existing = await _repo.GetByPartnerAndCountryAsync(
                       req.PartnerId, req.CountryId, ct);

        if (existing is not null)
            throw new PartnerCountryAlreadyExistException(req.PartnerId, req.CountryId);

        var entity = PartnerCountry.Create(
                         PartnerCountryId.Of(Guid.NewGuid()),
                         new PartnerId(req.PartnerId),
                         new CountryId(req.CountryId),
                         req.IsEnabled);

        await _repo.AddAsync(entity, ct);

        return Result.Success(entity.Id.Value);
    }
}
