using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.PartnerAggregate;
using wfc.referential.Domain.PartnerAggregate.Exceptions;
using wfc.referential.Domain.ParamTypeAggregate;
using wfc.referential.Domain.PartnerAccountAggregate;
using wfc.referential.Domain.SupportAccountAggregate;

namespace wfc.referential.Application.Partners.Commands.UpdatePartner;

public class UpdatePartnerCommandHandler : ICommandHandler<UpdatePartnerCommand, Result<bool>>
{
    private readonly IPartnerRepository _repo;
    private readonly IParamTypeRepository _paramTypeRepository;
    private readonly IPartnerAccountRepository _partnerAccountRepository;
    private readonly ISupportAccountRepository _supportAccountRepository;

    public UpdatePartnerCommandHandler(
        IPartnerRepository repo,
        IParamTypeRepository paramTypeRepository,
        IPartnerAccountRepository partnerAccountRepository,
        ISupportAccountRepository supportAccountRepository)
    {
        _repo = repo;
        _paramTypeRepository = paramTypeRepository;
        _partnerAccountRepository = partnerAccountRepository;
        _supportAccountRepository = supportAccountRepository;
    }

    public async Task<Result<bool>> Handle(UpdatePartnerCommand cmd, CancellationToken ct)
    {
        var partner = await _repo.GetByIdAsync(PartnerId.Of(cmd.PartnerId), ct);
        if (partner is null)
            throw new BusinessException($"Partner [{cmd.PartnerId}] not found.");

        // Check if code is unique (if changed)
        if (cmd.Code != partner.Code)
        {
            var existingByCode = await _repo.GetByConditionAsync(p => p.Code == cmd.Code, ct);
            if (existingByCode.Any())
                throw new PartnerCodeAlreadyExistException(cmd.Code);
        }

        // Check if tax identification number is unique (if changed)
        if (!string.IsNullOrEmpty(cmd.TaxIdentificationNumber) && cmd.TaxIdentificationNumber != partner.TaxIdentificationNumber)
        {
            var existingByTaxId = await _repo.GetByConditionAsync(p => p.TaxIdentificationNumber == cmd.TaxIdentificationNumber, ct);
            if (existingByTaxId.Any())
                throw new BusinessException($"Partner with tax identification number {cmd.TaxIdentificationNumber} already exists.");
        }

        // Check if ICE is unique (if changed)
        if (!string.IsNullOrEmpty(cmd.ICE) && cmd.ICE != partner.ICE)
        {
            var existingByICE = await _repo.GetByConditionAsync(p => p.ICE == cmd.ICE, ct);
            if (existingByICE.Any())
                throw new BusinessException($"Partner with ICE {cmd.ICE} already exists.");
        }

        // Validate NetworkMode exists if provided
        if (cmd.NetworkModeId.HasValue)
        {
            var networkMode = await _paramTypeRepository.GetByIdAsync(ParamTypeId.Of(cmd.NetworkModeId.Value), ct);
            if (networkMode is null)
                throw new BusinessException($"Network Mode with ID {cmd.NetworkModeId.Value} not found");
        }

        // Validate PaymentMode exists if provided
        if (cmd.PaymentModeId.HasValue)
        {
            var paymentMode = await _paramTypeRepository.GetByIdAsync(ParamTypeId.Of(cmd.PaymentModeId.Value), ct);
            if (paymentMode is null)
                throw new BusinessException($"Payment Mode with ID {cmd.PaymentModeId.Value} not found");
        }

        // Validate PartnerType exists if provided
        if (cmd.PartnerTypeId.HasValue)
        {
            var partnerType = await _paramTypeRepository.GetByIdAsync(ParamTypeId.Of(cmd.PartnerTypeId.Value), ct);
            if (partnerType is null)
                throw new BusinessException($"Partner Type with ID {cmd.PartnerTypeId.Value} not found");
        }

        // Validate SupportAccountType exists if provided
        if (cmd.SupportAccountTypeId.HasValue)
        {
            var supportAccountType = await _paramTypeRepository.GetByIdAsync(ParamTypeId.Of(cmd.SupportAccountTypeId.Value), ct);
            if (supportAccountType is null)
                throw new BusinessException($"Support Account Type with ID {cmd.SupportAccountTypeId.Value} not found");
        }

        // Validate CommissionAccount exists if provided
        if (cmd.CommissionAccountId.HasValue)
        {
            var commissionAccount = await _partnerAccountRepository.GetByIdAsync(PartnerAccountId.Of(cmd.CommissionAccountId.Value), ct);
            if (commissionAccount is null)
                throw new BusinessException($"Commission Account with ID {cmd.CommissionAccountId.Value} not found");
        }

        // Validate ActivityAccount exists if provided
        if (cmd.ActivityAccountId.HasValue)
        {
            var activityAccount = await _partnerAccountRepository.GetByIdAsync(PartnerAccountId.Of(cmd.ActivityAccountId.Value), ct);
            if (activityAccount is null)
                throw new BusinessException($"Activity Account with ID {cmd.ActivityAccountId.Value} not found");
        }

        // Validate SupportAccount exists if provided
        if (cmd.SupportAccountId.HasValue)
        {
            var supportAccount = await _supportAccountRepository.GetByIdAsync(SupportAccountId.Of(cmd.SupportAccountId.Value), ct);
            if (supportAccount is null)
                throw new BusinessException($"Support Account with ID {cmd.SupportAccountId.Value} not found");
        }

        // Validate Parent Partner exists if provided
        if (cmd.IdParent.HasValue)
        {
            var parentPartner = await _repo.GetByIdAsync(PartnerId.Of(cmd.IdParent.Value), ct);
            if (parentPartner is null)
                throw new BusinessException($"Parent Partner with ID {cmd.IdParent.Value} not found");
        }

        partner.Update(
            cmd.Code,
            cmd.Name,
            cmd.PersonType,
            cmd.ProfessionalTaxNumber,
            cmd.WithholdingTaxRate,
            cmd.HeadquartersCity,
            cmd.HeadquartersAddress,
            cmd.LastName,
            cmd.FirstName,
            cmd.PhoneNumberContact,
            cmd.MailContact,
            cmd.FunctionContact,
            cmd.TransferType,
            cmd.AuthenticationMode,
            cmd.TaxIdentificationNumber,
            cmd.TaxRegime,
            cmd.AuxiliaryAccount,
            cmd.ICE,
            cmd.Logo,
            cmd.IsEnabled);

        // Set relationships after validation
        if (cmd.NetworkModeId.HasValue)
            partner.SetNetworkMode(ParamTypeId.Of(cmd.NetworkModeId.Value));

        if (cmd.PaymentModeId.HasValue)
            partner.SetPaymentMode(ParamTypeId.Of(cmd.PaymentModeId.Value));

        if (cmd.PartnerTypeId.HasValue)
            partner.SetPartnerType(ParamTypeId.Of(cmd.PartnerTypeId.Value));

        if (cmd.SupportAccountTypeId.HasValue)
            partner.SetSupportAccountType(ParamTypeId.Of(cmd.SupportAccountTypeId.Value));

        if (cmd.CommissionAccountId.HasValue)
            partner.SetCommissionAccount(cmd.CommissionAccountId.Value);

        if (cmd.ActivityAccountId.HasValue)
            partner.SetActivityAccount(cmd.ActivityAccountId.Value);

        if (cmd.SupportAccountId.HasValue)
            partner.SetSupportAccount(cmd.SupportAccountId.Value);

        if (cmd.IdParent.HasValue)
            partner.SetParent(cmd.IdParent.Value);

        await _repo.SaveChangesAsync(ct);
        return Result.Success(true);
    }
}