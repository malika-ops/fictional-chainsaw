using BuildingBlocks.Core.Abstraction.CQRS;
using wfc.referential.Application.IdentityDocuments.Dtos;

namespace wfc.referential.Application.IdentityDocuments.Queries.GetIdentityDocumentById;

public record GetIdentityDocumentByIdQuery : IQuery<GetIdentityDocumentsResponse>
{
    public Guid IdentityDocumentId { get; init; }
} 