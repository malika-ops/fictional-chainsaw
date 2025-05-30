namespace wfc.referential.Application.CountryIdentityDocs.Dtos;

public record UpdateCountryIdentityDocRequest
{
    /// <summary>Country Identity Document association ID (from route).</summary>
    public Guid CountryIdentityDocId { get; init; }

    /// <summary>Country ID for the association.</summary>
    public Guid CountryId { get; init; }

    /// <summary>Identity Document ID for the association.</summary>
    public Guid IdentityDocumentId { get; init; }

    /// <summary>Association status (enabled/disabled).</summary>
    public bool IsEnabled { get; init; } = true;
}