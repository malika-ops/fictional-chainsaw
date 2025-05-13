using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Caching.Interface;
using wfc.referential.Domain.ProductAggregate;

namespace wfc.referential.Application.Products.Commands.CreateProduct;

public record CreateProductCommand : ICommand<Result<Guid>>, ICacheableQuery
{
    public ProductId ProductId { get; init; } = default!;
    public string Code { get; init; } = default!;
    public string Name { get; init; } = default!;
    public bool IsEnabled { get; init; } = true;

    public string CacheKey => $"{nameof(Product)}_{ProductId}";
    public int CacheExpiration => 5;

}