namespace wfc.referential.Application.CountryIdentityDocs.Dtos;

public record GetFiltredCountryIdentityDocsRequest : FilterRequest
{

    /// <summary>Filter by Country ID.</summary>
    public Guid? CountryId { get; init; }

    /// <summary>Filter by Identity Document ID.</summary>
    public Guid? IdentityDocumentId { get; init; }

}