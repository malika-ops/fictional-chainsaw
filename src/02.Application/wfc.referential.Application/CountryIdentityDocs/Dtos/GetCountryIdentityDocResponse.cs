namespace wfc.referential.Application.CountryIdentityDocs.Dtos;

public record GetCountryIdentityDocsResponse
{
    public Guid CountryIdentityDocId { get; init; }
    public Guid CountryId { get; init; }
    public Guid IdentityDocumentId { get; init; }
    public bool IsEnabled { get; init; }
}