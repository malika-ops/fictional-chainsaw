namespace wfc.referential.Application.PartnerCountries.Dtos;

public record PartnerCountryResponse
{
    public Guid PartnerCountryId { get; init; }
    public Guid PartnerId { get; init; }
    public Guid CountryId { get; init; }
    public bool IsEnabled { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset LastModified { get; init; }
    public string CreatedBy { get; init; } = string.Empty;
    public string LastModifiedBy { get; init; } = string.Empty;
}