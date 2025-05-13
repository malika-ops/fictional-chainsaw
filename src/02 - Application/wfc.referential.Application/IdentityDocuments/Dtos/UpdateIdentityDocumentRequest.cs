namespace wfc.referential.Application.IdentityDocuments.Dtos;

public record UpdateIdentityDocumentRequest
{
    public Guid IdentityDocumentId { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsEnabled { get; init; } = true;
}