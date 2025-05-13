using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Caching.Interface;
using wfc.referential.Domain.IdentityDocumentAggregate;

namespace wfc.referential.Application.IdentityDocuments.Commands.DeleteIdentityDocument;

public record DeleteIdentityDocumentCommand : ICommand<Result<bool>>, ICacheableQuery
{
    public Guid IdentityDocumentId { get; init; }
    public string CacheKey => $"{nameof(IdentityDocument)}_{IdentityDocumentId}";
    public int CacheExpiration => 5;
}