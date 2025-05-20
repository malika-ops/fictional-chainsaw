using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.PartnerAggregate;
using wfc.referential.Domain.PartnerAggregate.Exceptions;
using wfc.referential.Domain.PartnerAccountAggregate;
using wfc.referential.Domain.SupportAccountAggregate;

namespace wfc.referential.Application.Partners.Commands.UpdatePartner;

public record UpdatePartnerCommandHandler : ICommandHandler<UpdatePartnerCommand, Guid>
{
    private readonly IPartnerRepository _partnerRepository;
    private readonly IPartnerAccountRepository _partnerAccountRepository;
    private readonly ISupportAccountRepository _supportAccountRepository;

    public UpdatePartnerCommandHandler(
        IPartnerRepository partnerRepository,
        IPartnerAccountRepository partnerAccountRepository,
        ISupportAccountRepository supportAccountRepository)
    {
        _partnerRepository = partnerRepository;
        _partnerAccountRepository = partnerAccountRepository;
        _supportAccountRepository = supportAccountRepository;
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
        if (!string.IsNullOrEmpty(request.IdentificationNumber) && request.IdentificationNumber != partner.TaxIdentificationNumber)
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

        // Load related accounts if needed
        PartnerAccount commissionAccount = null;
        if (request.CommissionAccountId.HasValue && request.CommissionAccountId != partner.CommissionAccountId)
        {
            commissionAccount = await _partnerAccountRepository.GetByIdAsync(new PartnerAccountId(request.CommissionAccountId.Value), cancellationToken);
            if (commissionAccount is null)
                throw new BusinessException($"Commission Account with ID {request.CommissionAccountId} not found");
        }

        PartnerAccount activityAccount = null;
        if (request.ActivityAccountId.HasValue && request.ActivityAccountId != partner.ActivityAccountId)
        {
            activityAccount = await _partnerAccountRepository.GetByIdAsync(new PartnerAccountId(request.ActivityAccountId.Value), cancellationToken);
            if (activityAccount is null)
                throw new BusinessException($"Activity Account with ID {request.ActivityAccountId} not found");
        }

        SupportAccount supportAccount = null;
        if (request.SupportAccountId.HasValue && request.SupportAccountId != partner.SupportAccountId)
        {
            supportAccount = await _supportAccountRepository.GetByIdAsync(new SupportAccountId(request.SupportAccountId.Value), cancellationToken);
            if (supportAccount is null)
                throw new BusinessException($"Support Account with ID {request.SupportAccountId} not found");
        }

        // Update the partner
        partner.Update(
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