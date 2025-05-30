namespace wfc.referential.Application.IdentityDocuments.Dtos;

public record CreateIdentityDocumentRequest
{
    /// <summary>Unique identity document code.</summary>
    /// <example>PASSPORT</example>
    public string Code { get; init; } = string.Empty;

    /// <summary>Identity document name.</summary>
    /// <example>Passport</example>
    public string Name { get; init; } = string.Empty;

    /// <summary>Identity document description.</summary>
    /// <example>Official travel document</example>
    public string? Description { get; init; }
}