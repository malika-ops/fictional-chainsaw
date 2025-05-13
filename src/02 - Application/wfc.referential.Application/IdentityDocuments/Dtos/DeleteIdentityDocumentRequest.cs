using System.ComponentModel.DataAnnotations;

namespace wfc.referential.Application.IdentityDocuments.Dtos;

public record DeleteIdentityDocumentRequest
{
    public Guid IdentityDocumentId { get; init; }
}