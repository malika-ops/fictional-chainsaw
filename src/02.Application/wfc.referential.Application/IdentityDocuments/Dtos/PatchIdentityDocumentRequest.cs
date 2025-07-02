namespace wfc.referential.Application.IdentityDocuments.Dtos;

public record PatchIdentityDocumentRequest
{
    /// <summary>Unique identity document code.</summary>
    public string? Code { get; init; }

    /// <summary>Display name.</summary>
    public string? Name { get; init; }

    /// <summary>Description.</summary>
    public string? Description { get; init; }

    /// <summary>Identity document status (enabled/disabled).</summary>
    public bool? IsEnabled { get; init; }
}