using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Caching.Interface;
using wfc.referential.Domain.IdentityDocumentAggregate;

namespace wfc.referential.Application.IdentityDocuments.Commands.PatchIdentityDocument;

public record PatchIdentityDocumentCommand : ICommand<Result<Guid>>, ICacheableQuery
{
    public Guid IdentityDocumentId { get; init; }
    public string? Code { get; init; }
    public string? Name { get; init; }
    public string? Description { get; init; }
    public bool? IsEnabled { get; init; }

    public string CacheKey => $"{nameof(IdentityDocument)}_{IdentityDocumentId}";
    public int CacheExpiration => 5;
}
