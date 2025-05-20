using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Caching.Interface;
using wfc.referential.Domain.ProductAggregate;

namespace wfc.referential.Application.Products.Commands.UpdateProduct;

public record UpdateProductCommand : ICommand<Result<Guid>>, ICacheableQuery
{
    public Guid ProductId { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public bool IsEnabled { get; set; }
    public string CacheKey => $"{nameof(Product)}_{ProductId}";
    public int CacheExpiration => 5;
}
