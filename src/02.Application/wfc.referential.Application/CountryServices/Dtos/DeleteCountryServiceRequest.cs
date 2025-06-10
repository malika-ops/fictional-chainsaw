namespace wfc.referential.Application.CountryServices.Dtos;

public record DeleteCountryServiceRequest
{
    /// <summary>GUID of the country Service association to delete.</summary>
    public Guid CountryServiceId { get; init; }
}