namespace wfc.referential.Application.Partners.Dtos;

public record PartnerResponse(
    Guid PartnerId,
    string Code,
    string Label,
    string NetworkMode,
    string PaymentMode,
    string IdPartner,
    string SupportAccountType,
    string IdentificationNumber,
    string TaxRegime,
    string AuxiliaryAccount,
    string ICE,
    bool IsEnabled,
    string Logo,
    Guid SectorId,
    string SectorName,
    Guid CityId,
    string CityName);