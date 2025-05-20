using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.PartnerAggregate;
using wfc.referential.Domain.PartnerAggregate.Exceptions;
using wfc.referential.Domain.PartnerAccountAggregate;
using wfc.referential.Domain.SupportAccountAggregate;

namespace wfc.referential.Application.Partners.Commands.PatchPartner;

public record PatchPartnerCommandHandler : ICommandHandler<PatchPartnerCommand, Guid>
{
    private readonly IPartnerRepository _partnerRepository;
    private readonly IPartnerAccountRepository _partnerAccountRepository;
    private readonly ISupportAccountRepository _supportAccountRepository;

    public PatchPartnerCommandHandler(
        IPartnerRepository partnerRepository,
        IPartnerAccountRepository partnerAccountRepository,
        ISupportAccountRepository supportAccountRepository)
    {
        _partnerRepository = partnerRepository;
        _partnerAccountRepository = partnerAccountRepository;
        _supportAccountRepository = supportAccountRepository;
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
        if (request.IdentificationNumber != null && request.IdentificationNumber != partner.TaxIdentificationNumber)
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

        // Collect updates for domain entity
        var updatedCode = request.Code ?? partner.Code;
        var updatedLabel = request.Label ?? partner.Label;
        var updatedNetworkMode = request.NetworkMode ?? partner.NetworkMode;
        var updatedPaymentMode = request.PaymentMode ?? partner.PaymentMode;
        var updatedType = request.Type ?? partner.Type;
        var updatedSupportAccountType = request.SupportAccountType ?? partner.SupportAccountType;
        var updatedIdentificationNumber = request.IdentificationNumber ?? partner.TaxIdentificationNumber;
        var updatedTaxRegime = request.TaxRegime ?? partner.TaxRegime;
        var updatedAuxiliaryAccount = request.AuxiliaryAccount ?? partner.AuxiliaryAccount;
        var updatedICE = request.ICE ?? partner.ICE;
        var updatedRASRate = request.RASRate ?? partner.RASRate;
        var updatedLogo = request.Logo ?? partner.Logo;
        var updatedIdParent = request.IdParent ?? partner.IdParent;
        var updatedCommissionAccountId = request.CommissionAccountId ?? partner.CommissionAccountId;
        var updatedActivityAccountId = request.ActivityAccountId ?? partner.ActivityAccountId;
        var updatedSupportAccountId = request.SupportAccountId ?? partner.SupportAccountId;

        // Update via domain methods
        partner.Patch(
            updatedCode,
            updatedLabel,
            updatedNetworkMode,
            updatedPaymentMode,
            updatedType,
            updatedSupportAccountType,
            updatedIdentificationNumber,
            updatedTaxRegime,
            updatedAuxiliaryAccount,
            updatedICE,
            updatedRASRate,
            updatedLogo,
            updatedIdParent,
            updatedCommissionAccountId,
            updatedActivityAccountId,
            updatedSupportAccountId
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