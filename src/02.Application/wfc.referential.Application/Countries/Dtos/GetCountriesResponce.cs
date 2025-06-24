using wfc.referential.Application.Currencies.Dtos;

namespace wfc.referential.Application.Countries.Dtos;

public record GetCountriesResponce
{
    public Guid Id { get; init; }
    public string Abbreviation { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string ISO2 { get; init; } = string.Empty;
    public string ISO3 { get; init; } = string.Empty;
    public string DialingCode { get; init; } = string.Empty;
    public string TimeZone { get; init; } = string.Empty;
    public bool HasSector { get; init; }
    public bool IsSmsEnabled { get; init; }
    public int NumberDecimalDigits { get; init; }
    public bool IsEnabled { get; init; }
    public GetCurrenciesResponse Currency { get; init; } = default!;
    public Guid MonetaryZoneId { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset LastModified { get; init; }
    public string CreatedBy { get; init; } = string.Empty;
    public string LastModifiedBy { get; init; } = string.Empty;
}
