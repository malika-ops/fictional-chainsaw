using System.ComponentModel.DataAnnotations;

namespace wfc.referential.Application.IdentityDocuments.Dtos;

public record PatchIdentityDocumentRequest
{
    public Guid IdentityDocumentId { get; init; }
    public string? Code { get; init; }
    public string? Name { get; init; }
    public string? Description { get; init; }
    public bool? IsEnabled { get; init; }
}