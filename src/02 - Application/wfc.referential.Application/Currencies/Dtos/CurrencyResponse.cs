namespace wfc.referential.Application.Currencies.Dtos;

public record CurrencyResponse(
    Guid CurrencyId,
    string Code,
    string CodeAR,
    string CodeEN,
    string Name,
    int CodeIso,
    bool IsEnabled,
    int CountriesCount
);