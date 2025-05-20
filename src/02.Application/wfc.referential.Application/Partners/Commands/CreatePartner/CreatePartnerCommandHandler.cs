using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.PartnerAggregate;
using wfc.referential.Domain.PartnerAggregate.Exceptions;
using wfc.referential.Domain.PartnerAccountAggregate;
using wfc.referential.Domain.SupportAccountAggregate;

namespace wfc.referential.Application.Partners.Commands.CreatePartner;

public record CreatePartnerCommandHandler : ICommandHandler<CreatePartnerCommand, Result<Guid>>
{
    private readonly IPartnerRepository _partnerRepository;
    private readonly IPartnerAccountRepository _partnerAccountRepository;
    private readonly ISupportAccountRepository _supportAccountRepository;

    public CreatePartnerCommandHandler(
        IPartnerRepository partnerRepository,
        IPartnerAccountRepository partnerAccountRepository,
        ISupportAccountRepository supportAccountRepository)
    {
        _partnerRepository = partnerRepository;
        _partnerAccountRepository = partnerAccountRepository;
        _supportAccountRepository = supportAccountRepository;
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

        // Load related accounts if needed
        PartnerAccount commissionAccount = null;
        if (request.CommissionAccountId.HasValue)
        {
            commissionAccount = await _partnerAccountRepository.GetByIdAsync(new PartnerAccountId(request.CommissionAccountId.Value), cancellationToken);
            if (commissionAccount is null)
                throw new BusinessException($"Commission Account with ID {request.CommissionAccountId} not found");
        }

        PartnerAccount activityAccount = null;
        if (request.ActivityAccountId.HasValue)
        {
            activityAccount = await _partnerAccountRepository.GetByIdAsync(new PartnerAccountId(request.ActivityAccountId.Value), cancellationToken);
            if (activityAccount is null)
                throw new BusinessException($"Activity Account with ID {request.ActivityAccountId} not found");
        }

        SupportAccount supportAccount = null;
        if (request.SupportAccountId.HasValue)
        {
            supportAccount = await _supportAccountRepository.GetByIdAsync(new SupportAccountId(request.SupportAccountId.Value), cancellationToken);
            if (supportAccount is null)
                throw new BusinessException($"Support Account with ID {request.SupportAccountId} not found");
        }

        var id = PartnerId.Of(Guid.NewGuid());
        var partner = Partner.Create(
            id,
            request.Code,
            request.Label,
            request.NetworkMode,
            request.PaymentMode,
            request.Type,
            request.SupportAccountType,
            request.IdentificationNumber,
            request.TaxRegime,
            request.AuxiliaryAccount,
            request.ICE,
            request.RASRate,
            request.Logo,
            request.IdParent,
            request.CommissionAccountId,
            request.ActivityAccountId,
            request.SupportAccountId
        );

        // Set account relationships if needed
        if (commissionAccount != null)
        {
            partner.SetCommissionAccount(request.CommissionAccountId.Value, commissionAccount);
        }

        if (activityAccount != null)
        {
            partner.SetActivityAccount(request.ActivityAccountId.Value, activityAccount);
        }

        if (supportAccount != null)
        {
            partner.SetSupportAccount(request.SupportAccountId.Value, supportAccount);
        }

        await _partnerRepository.AddPartnerAsync(partner, cancellationToken);

        return Result.Success(partner.Id.Value);
    }
}