namespace wfc.referential.Application.PartnerCountries.Dtos;

public record GetFiltredPartnerCountriesRequest : FilterRequest
{
    /// <summary>Filter by CountryId.</summary>
    public Guid? CountryId { get; init; }
}
