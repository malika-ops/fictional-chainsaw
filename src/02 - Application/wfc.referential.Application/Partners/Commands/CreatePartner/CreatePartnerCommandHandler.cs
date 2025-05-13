using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.CityAggregate;
using wfc.referential.Domain.PartnerAggregate;
using wfc.referential.Domain.PartnerAggregate.Exceptions;
using wfc.referential.Domain.SectorAggregate;

namespace wfc.referential.Application.Partners.Commands.CreatePartner;

public record CreatePartnerCommandHandler : ICommandHandler<CreatePartnerCommand, Result<Guid>>
{
    private readonly IPartnerRepository _partnerRepository;
    private readonly ISectorRepository _sectorRepository;
    private readonly ICityRepository _cityRepository;

    public CreatePartnerCommandHandler(
        IPartnerRepository partnerRepository,
        ISectorRepository sectorRepository,
        ICityRepository cityRepository)
    {
        _partnerRepository = partnerRepository;
        _sectorRepository = sectorRepository;
        _cityRepository = cityRepository;
    }

    public async Task<Result<Guid>> Handle(CreatePartnerCommand request, CancellationToken cancellationToken)
    {
        // Check if the code already exists
        var existingCode = await _partnerRepository.GetByCodeAsync(request.Code, cancellationToken);
        if (existingCode is not null)
            throw new PartnerAlreadyExistException(request.Code);

        // Check if the identification number already exists
        if (!string.IsNullOrEmpty(request.IdentificationNumber))
        {
            var existingIdentificationNumber = await _partnerRepository.GetByIdentificationNumberAsync(request.IdentificationNumber, cancellationToken);
            if (existingIdentificationNumber is not null)
                throw new BusinessException($"Partner with identification number {request.IdentificationNumber} already exists.");
        }

        // Check if the ICE already exists
        if (!string.IsNullOrEmpty(request.ICE))
        {
            var existingICE = await _partnerRepository.GetByICEAsync(request.ICE, cancellationToken);
            if (existingICE is not null)
                throw new BusinessException($"Partner with ICE {request.ICE} already exists.");
        }

        // Check if the Sector exists
        var sector = await _sectorRepository.GetByIdAsync(new SectorId(request.SectorId), cancellationToken);
        if (sector is null)
            throw new BusinessException($"Sector with ID {request.SectorId} not found");

        // Check if the City exists
        var city = await _cityRepository.GetByIdAsync(request.CityId, cancellationToken);
        if (city is null)
            throw new BusinessException($"City with ID {request.CityId} not found");

        var id = PartnerId.Of(Guid.NewGuid());
        var partner = Partner.Create(
            id,
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

        await _partnerRepository.AddPartnerAsync(partner, cancellationToken);

        return Result.Success(partner.Id.Value);
    }
}