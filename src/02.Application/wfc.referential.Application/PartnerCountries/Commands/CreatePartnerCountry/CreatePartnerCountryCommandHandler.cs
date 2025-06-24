using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.PartnerAggregate;
using wfc.referential.Domain.PartnerCountryAggregate;
using wfc.referential.Domain.PartnerCountryAggregate.Exceptions;

namespace wfc.referential.Application.PartnerCountries.Commands.CreatePartnerCountry;

public class CreatePartnerCountryCommandHandler : ICommandHandler<CreatePartnerCountryCommand, Result<Guid>>
{
    private readonly IPartnerCountryRepository _repo;
    private readonly ICountryRepository _countryRepo;
    private readonly IPartnerRepository _partnerRepo;

    public CreatePartnerCountryCommandHandler(IPartnerCountryRepository repo, ICountryRepository countryRepo, IPartnerRepository partnerRepo)
    {
        _repo = repo;
        _countryRepo = countryRepo;
        _partnerRepo = partnerRepo;
    }

    public async Task<Result<Guid>> Handle(CreatePartnerCountryCommand req,
                                           CancellationToken ct)
    {
        var partnerCountryId = PartnerCountryId.Of(Guid.NewGuid());
        var countryId = CountryId.Of(req.CountryId);
        var partnerId = PartnerId.Of(req.PartnerId);

        var existing = await _repo.GetOneByConditionAsync( p => p.PartnerId == partnerId &&
                                                      p.CountryId == countryId, ct);

        if (existing is not null)
            throw new PartnerCountryAlreadyExistException(req.PartnerId, req.CountryId);

        var partner = await _partnerRepo.GetByIdAsync(partnerId, ct) 
            ?? throw new ResourceNotFoundException($"Partner with Id '{req.PartnerId}' not found");

        var country = await _countryRepo.GetByIdAsync(countryId, ct) 
            ?? throw new ResourceNotFoundException($"Country with Id '{req.CountryId}' not found");

        var entity = PartnerCountry.Create(
                         partnerCountryId,
                         partnerId,
                         countryId);

        await _repo.AddAsync(entity, ct);

        await _repo.SaveChangesAsync(ct);

        return Result.Success(entity.Id!.Value);
    }
}
