using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Caching.Interface;
using wfc.referential.Domain.ProductAggregate;

namespace wfc.referential.Application.Products.Commands.DeleteProduct;

public record DeleteProductCommand : ICommand<Result<bool>>, ICacheableQuery
{
    public Guid ProductId { get; init; }
    public string CacheKey => $"{nameof(Product)}_{ProductId}";
    public int CacheExpiration => 5;
}