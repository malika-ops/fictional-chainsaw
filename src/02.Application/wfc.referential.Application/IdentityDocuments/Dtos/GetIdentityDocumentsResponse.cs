namespace wfc.referential.Application.IdentityDocuments.Dtos;

public record GetIdentityDocumentsResponse
{
    /// <summary>
    /// Unique identifier of the identity document.
    /// </summary>
    /// <example>e2a1c3b4-5d6f-4a7b-8c9d-0e1f2a3b4c5d</example>
    public Guid IdentityDocumentId { get; init; }

    /// <summary>
    /// Unique code of the identity document.
    /// </summary>
    /// <example>CIN</example>
    public string Code { get; init; } = string.Empty;

    /// <summary>
    /// Name of the identity document.
    /// </summary>
    /// <example>Carte d'Identité Nationale</example>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Description of the identity document.
    /// </summary>
    /// <example>National identity card issued by the government</example>
    public string? Description { get; init; }

    /// <summary>
    /// Indicates whether the identity document is enabled ("true" or "false" as string).
    /// </summary>
    /// <example>true</example>
    public string IsEnabled { get; init; } = string.Empty;
}