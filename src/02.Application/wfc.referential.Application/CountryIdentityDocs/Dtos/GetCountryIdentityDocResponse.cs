namespace wfc.referential.Application.CountryIdentityDocs.Dtos;

public record GetCountryIdentityDocsResponse
{
    /// <summary>
    /// Unique identifier of the country identity document association.
    /// </summary>
    /// <example>e2a1c3b4-5d6f-4a7b-8c9d-0e1f2a3b4c5d</example>
    public Guid CountryIdentityDocId { get; init; }

    /// <summary>
    /// Unique identifier of the country.
    /// </summary>
    /// <example>f1e2d3c4-b5a6-7890-1234-56789abcdef0</example>
    public Guid CountryId { get; init; }

    /// <summary>
    /// Unique identifier of the identity document.
    /// </summary>
    /// <example>0a1b2c3d-4e5f-6789-abcd-ef0123456789</example>
    public Guid IdentityDocumentId { get; init; }

    /// <summary>
    /// Indicates whether the association is enabled.
    /// </summary>
    /// <example>true</example>
    public bool IsEnabled { get; init; }
}