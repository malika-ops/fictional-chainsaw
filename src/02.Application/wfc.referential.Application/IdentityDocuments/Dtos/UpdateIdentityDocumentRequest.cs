namespace wfc.referential.Application.IdentityDocuments.Dtos;

public record UpdateIdentityDocumentRequest
{
    /// <summary>Identity document GUID (from route).</summary>
    public Guid IdentityDocumentId { get; init; }

    /// <summary>Unique identity document code.</summary>
    public string Code { get; init; } = string.Empty;

    /// <summary>Display name.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Description.</summary>
    public string? Description { get; init; }

    /// <summary>Identity document status (enabled/disabled).</summary>
    public bool IsEnabled { get; init; } = true;
}