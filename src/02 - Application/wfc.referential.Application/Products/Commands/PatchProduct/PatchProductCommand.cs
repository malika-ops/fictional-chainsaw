using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Caching.Interface;
using wfc.referential.Domain.ProductAggregate;

namespace wfc.referential.Application.Products.Commands.PatchProduct;

public record PatchProductCommand : ICommand<Result<Guid>>, ICacheableQuery
{
    // The ID from the route
    public Guid ProductId { get; init; }

    // The optional fields to Product
    public string? Code { get; init; }
    public string? Name { get; init; }
    public bool? IsEnabled { get; init; }

    public string CacheKey => $"{nameof(Product)}_{ProductId}";
    public int CacheExpiration => 5;
}
