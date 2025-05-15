namespace wfc.referential.Application.Partners.Dtos;

public record PartnerResponse(
    Guid PartnerId,
    string Code,
    string Label,
    string Type,
    string NetworkMode,
    string PaymentMode,
    Guid? IdParent,
    string SupportAccountType,
    string TaxIdentificationNumber,
    string TaxRegime,
    string AuxiliaryAccount,
    string ICE,
    string RASRate,
    bool IsEnabled,
    string Logo,
    Guid? CommissionAccountId,
    string CommissionAccountName,
    Guid? ActivityAccountId,
    string ActivityAccountName,
    Guid? SupportAccountId,
    string SupportAccountName);