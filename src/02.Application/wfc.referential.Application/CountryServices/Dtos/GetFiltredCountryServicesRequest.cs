namespace wfc.referential.Application.CountryServices.Dtos;

public record GetFiltredCountryServicesRequest : FilterRequest
{

    /// <summary>Filter by Country ID.</summary>
    public Guid? CountryId { get; init; }

    /// <summary>Filter by Service ID.</summary>
    public Guid? ServiceId { get; init; }
}