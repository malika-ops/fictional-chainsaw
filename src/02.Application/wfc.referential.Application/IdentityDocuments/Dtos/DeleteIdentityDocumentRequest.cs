namespace wfc.referential.Application.IdentityDocuments.Dtos;

public record DeleteIdentityDocumentRequest
{
    /// <summary>GUID of the identity document to delete.</summary>
    public Guid IdentityDocumentId { get; init; }
}