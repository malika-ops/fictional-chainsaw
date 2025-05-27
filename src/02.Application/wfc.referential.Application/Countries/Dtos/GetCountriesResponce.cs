using wfc.referential.Application.Currencies.Dtos;

namespace wfc.referential.Application.Countries.Dtos;

public record GetCountriesResponce ( 
    Guid CountryId,
    string Abbreviation,
    string Name,
    string Code,
    string ISO2, 
    string ISO3,
    string DialingCode,
    string TimeZone,
    bool HasSector,
    bool IsSmsEnabled,
    int NumberDecimalDigits,
    bool IsEnabled,
    GetCurrenciesResponse Currency,
    Guid MonetaryZoneId
    );
