using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.CityAggregate;
using wfc.referential.Domain.PartnerAggregate;
using wfc.referential.Domain.PartnerAggregate.Exceptions;
using wfc.referential.Domain.SectorAggregate;

namespace wfc.referential.Application.Partners.Commands.PatchPartner;

public record PatchPartnerCommandHandler : ICommandHandler<PatchPartnerCommand, Guid>
{
    private readonly IPartnerRepository _partnerRepository;
    private readonly ISectorRepository _sectorRepository;
    private readonly ICityRepository _cityRepository;

    public PatchPartnerCommandHandler(
        IPartnerRepository partnerRepository,
        ISectorRepository sectorRepository,
        ICityRepository cityRepository)
    {
        _partnerRepository = partnerRepository;
        _sectorRepository = sectorRepository;
        _cityRepository = cityRepository;
    }

    public async Task<Guid> Handle(PatchPartnerCommand request, CancellationToken cancellationToken)
    {
        var partner = await _partnerRepository.GetByIdAsync(new PartnerId(request.PartnerId), cancellationToken);
        if (partner == null)
            throw new BusinessException("Partner not found");

        // Check if code is unique if it's being updated
        if (request.Code != null && request.Code != partner.Code)
        {
            var existingWithCode = await _partnerRepository.GetByCodeAsync(request.Code, cancellationToken);
            if (existingWithCode != null && existingWithCode.Id.Value != request.PartnerId)
                throw new PartnerAlreadyExistException(request.Code);
        }

        // Check if identification number is unique if it's being updated
        if (request.IdentificationNumber != null && request.IdentificationNumber != partner.IdentificationNumber)
        {
            var existingWithIdentificationNumber = await _partnerRepository.GetByIdentificationNumberAsync(request.IdentificationNumber, cancellationToken);
            if (existingWithIdentificationNumber != null && existingWithIdentificationNumber.Id.Value != request.PartnerId)
                throw new BusinessException($"Partner with identification number {request.IdentificationNumber} already exists.");
        }

        // Check if ICE is unique if it's being updated
        if (request.ICE != null && request.ICE != partner.ICE)
        {
            var existingWithICE = await _partnerRepository.GetByICEAsync(request.ICE, cancellationToken);
            if (existingWithICE != null && existingWithICE.Id.Value != request.PartnerId)
                throw new BusinessException($"Partner with ICE {request.ICE} already exists.");
        }

        // Get updated sector if needed
        var sector = partner.Sector;
        if (request.SectorId.HasValue && request.SectorId.Value != partner.Sector.Id.Value)
        {
            var updatedSector = await _sectorRepository.GetByIdAsync(new SectorId(request.SectorId.Value), cancellationToken);
            if (updatedSector == null)
                throw new BusinessException($"Sector with ID {request.SectorId} not found");
            sector = updatedSector;
        }

        // Get updated city if needed
        var city = partner.City;
        if (request.CityId.HasValue && request.CityId.Value != partner.City.Id.Value)
        {
            var updatedCity = await _cityRepository.GetByIdAsync(request.CityId.Value, cancellationToken);
            if (updatedCity == null)
                throw new BusinessException($"City with ID {request.CityId} not found");
            city = updatedCity;
        }

        // Collect updates for domain entities
        var updatedCode = request.Code ?? partner.Code;
        var updatedLabel = request.Label ?? partner.Label;
        var updatedNetworkMode = request.NetworkMode ?? partner.NetworkMode;
        var updatedPaymentMode = request.PaymentMode ?? partner.PaymentMode;
        var updatedIdPartner = request.IdPartner ?? partner.IdPartner;
        var updatedSupportAccountType = request.SupportAccountType ?? partner.SupportAccountType;
        var updatedIdentificationNumber = request.IdentificationNumber ?? partner.IdentificationNumber;
        var updatedTaxRegime = request.TaxRegime ?? partner.TaxRegime;
        var updatedAuxiliaryAccount = request.AuxiliaryAccount ?? partner.AuxiliaryAccount;
        var updatedICE = request.ICE ?? partner.ICE;
        var updatedLogo = request.Logo ?? partner.Logo;

        // Update via domain methods
        partner.Patch(
            updatedCode,
            updatedLabel,
            updatedNetworkMode,
            updatedPaymentMode,
            updatedIdPartner,
            updatedSupportAccountType,
            updatedIdentificationNumber,
            updatedTaxRegime,
            updatedAuxiliaryAccount,
            updatedICE,
            updatedLogo,
            sector,
            city
        );

        // Handle IsEnabled status changes separately through the proper domain methods
        if (request.IsEnabled.HasValue)
        {
            if (request.IsEnabled.Value && !partner.IsEnabled)
            {
                partner.Activate();
            }
            else if (!request.IsEnabled.Value && partner.IsEnabled)
            {
                partner.Disable();
            }
        }

        await _partnerRepository.UpdatePartnerAsync(partner, cancellationToken);

        return partner.Id.Value;
    }
}