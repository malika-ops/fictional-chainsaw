using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Caching.Interface;
using wfc.referential.Domain.IdentityDocumentAggregate;

namespace wfc.referential.Application.IdentityDocuments.Commands.UpdateIdentityDocument;

public record UpdateIdentityDocumentCommand : ICommand<Result<Guid>>, ICacheableQuery
{
    public Guid IdentityDocumentId { get; init; }
    public string Code { get; init; } = default!;
    public string Name { get; init; } = default!;
    public string? Description { get; init; }
    public bool IsEnabled { get; init; } = true;

    public string CacheKey => $"{nameof(IdentityDocument)}_{IdentityDocumentId}";
    public int CacheExpiration => 5;
}