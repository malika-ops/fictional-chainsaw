using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.PartnerAggregate;
using wfc.referential.Domain.PartnerCountryAggregate;
using wfc.referential.Domain.PartnerCountryAggregate.Exceptions;

namespace wfc.referential.Application.PartnerCountries.Commands.UpdatePartnerCountry;

public class UpdatePartnerCountryCommandHandler : ICommandHandler<UpdatePartnerCountryCommand, Result<bool>>
{
    private readonly IPartnerCountryRepository _partnerCountryRepo;
    private readonly IPartnerRepository _partnerRepository;
    private readonly ICountryRepository _countryRepository;

    public UpdatePartnerCountryCommandHandler(IPartnerCountryRepository partnerCountryRepo,
        IPartnerRepository partnerRepository,
        ICountryRepository countryRepository)
    {
        _partnerCountryRepo = partnerCountryRepo;
        _partnerRepository = partnerRepository;
        _countryRepository = countryRepository;
    }

    public async Task<Result<bool>> Handle(UpdatePartnerCountryCommand req, CancellationToken ct)
    {
        var partnerCountryId = PartnerCountryId.Of(req.PartnerCountryId);
        var partnerId = PartnerId.Of(req.PartnerId);
        var countryId = CountryId.Of(req.CountryId);

        var entity = await _partnerCountryRepo.GetByIdAsync(partnerCountryId, ct)
                    ?? throw new ResourceNotFoundException("PartnerCountry not found");

        var partner = await _partnerRepository.GetByIdAsync(partnerId, ct)
           ?? throw new ResourceNotFoundException($"Partner with Id '{req.PartnerId}' not found");

        var country = await _countryRepository.GetByIdAsync(countryId, ct)
            ?? throw new ResourceNotFoundException($"Country with Id '{req.CountryId}' not found");

        var duplicate = await _partnerCountryRepo.GetOneByConditionAsync( r => r.PartnerId == partnerId &&
                        r.CountryId == countryId, ct);
        if (duplicate is not null && duplicate.Id != entity.Id)
            throw new PartnerCountryAlreadyExistException(req.PartnerId, req.CountryId);

        entity.Update(partnerId,
                      countryId,
                      req.IsEnabled);

        await _partnerCountryRepo.SaveChangesAsync(ct);

        return Result.Success(true);
    }
}
