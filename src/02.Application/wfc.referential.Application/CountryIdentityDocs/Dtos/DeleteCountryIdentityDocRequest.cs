namespace wfc.referential.Application.CountryIdentityDocs.Dtos;

public record DeleteCountryIdentityDocRequest
{
    /// <summary>GUID of the country identity document association to delete.</summary>
    public Guid CountryIdentityDocId { get; init; }
}