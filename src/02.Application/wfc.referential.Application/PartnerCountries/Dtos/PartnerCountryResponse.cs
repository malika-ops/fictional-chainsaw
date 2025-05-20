namespace wfc.referential.Application.PartnerCountries.Dtos;

public record PartnerCountryResponse(
    Guid PartnerCountryId,
    Guid PartnerId,
    Guid CountryId,
    bool IsEnabled);