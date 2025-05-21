namespace wfc.referential.Application.IdentityDocuments.Dtos;

public record CreateIdentityDocumentRequest
{
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
}