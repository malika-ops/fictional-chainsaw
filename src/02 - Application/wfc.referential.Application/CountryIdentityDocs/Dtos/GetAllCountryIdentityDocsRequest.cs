namespace wfc.referential.Application.CountryIdentityDocs.Dtos;

public record GetAllCountryIdentityDocsRequest
{
    /// <summary>Optional page number (default = 1).</summary>
    public int? PageNumber { get; init; } = 1;

    /// <summary>Optional page size (default = 10).</summary>
    public int? PageSize { get; init; } = 10;

    /// <summary>Optional filter by Country ID.</summary>
    public Guid? CountryId { get; init; }

    /// <summary>Optional filter by Identity Document ID.</summary>
    public Guid? IdentityDocumentId { get; init; }

    /// <summary>Optional filter by enabled status.</summary>
    public bool? IsEnabled { get; init; } = true;
}