using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.CityAggregate;
using wfc.referential.Domain.PartnerAggregate;
using wfc.referential.Domain.PartnerAggregate.Exceptions;
using wfc.referential.Domain.SectorAggregate;

namespace wfc.referential.Application.Partners.Commands.UpdatePartner;

public record UpdatePartnerCommandHandler : ICommandHandler<UpdatePartnerCommand, Guid>
{
    private readonly IPartnerRepository _partnerRepository;
    private readonly ISectorRepository _sectorRepository;
    private readonly ICityRepository _cityRepository;

    public UpdatePartnerCommandHandler(
        IPartnerRepository partnerRepository,
        ISectorRepository sectorRepository,
        ICityRepository cityRepository)
    {
        _partnerRepository = partnerRepository;
        _sectorRepository = sectorRepository;
        _cityRepository = cityRepository;
    }

    public async Task<Guid> Handle(UpdatePartnerCommand request, CancellationToken cancellationToken)
    {
        // Check if partner exists
        var partner = await _partnerRepository.GetByIdAsync(new PartnerId(request.PartnerId), cancellationToken);
        if (partner is null)
            throw new BusinessException($"Partner with ID {request.PartnerId} not found");

        // Check if code is unique (if changed)
        if (request.Code != partner.Code)
        {
            var existingWithCode = await _partnerRepository.GetByCodeAsync(request.Code, cancellationToken);
            if (existingWithCode is not null && existingWithCode.Id.Value != request.PartnerId)
                throw new PartnerAlreadyExistException(request.Code);
        }

        // Check if identification number is unique (if changed)
        if (!string.IsNullOrEmpty(request.IdentificationNumber) && request.IdentificationNumber != partner.IdentificationNumber)
        {
            var existingWithIdentificationNumber = await _partnerRepository.GetByIdentificationNumberAsync(request.IdentificationNumber, cancellationToken);
            if (existingWithIdentificationNumber is not null && existingWithIdentificationNumber.Id.Value != request.PartnerId)
                throw new BusinessException($"Partner with identification number {request.IdentificationNumber} already exists.");
        }

        // Check if ICE is unique (if changed)
        if (!string.IsNullOrEmpty(request.ICE) && request.ICE != partner.ICE)
        {
            var existingWithICE = await _partnerRepository.GetByICEAsync(request.ICE, cancellationToken);
            if (existingWithICE is not null && existingWithICE.Id.Value != request.PartnerId)
                throw new BusinessException($"Partner with ICE {request.ICE} already exists.");
        }

        // Get the sector
        var sector = await _sectorRepository.GetByIdAsync(new SectorId(request.SectorId), cancellationToken);
        if (sector is null)
            throw new BusinessException($"Sector with ID {request.SectorId} not found");

        // Get the city
        var city = await _cityRepository.GetByIdAsync(request.CityId, cancellationToken);
        if (city is null)
            throw new BusinessException($"City with ID {request.CityId} not found");

        // Update the partner
        partner.Update(
            request.Code,
            request.Label,
            request.NetworkMode,
            request.PaymentMode,
            request.IdPartner,
            request.SupportAccountType,
            request.IdentificationNumber,
            request.TaxRegime,
            request.AuxiliaryAccount,
            request.ICE,
            request.Logo,
            sector,
            city
        );

        if (request.IsEnabled && !partner.IsEnabled)
        {
            partner.Activate();
        }
        else if (!request.IsEnabled && partner.IsEnabled)
        {
            partner.Disable();
        }

        await _partnerRepository.UpdatePartnerAsync(partner, cancellationToken);

        return partner.Id.Value;
    }
}