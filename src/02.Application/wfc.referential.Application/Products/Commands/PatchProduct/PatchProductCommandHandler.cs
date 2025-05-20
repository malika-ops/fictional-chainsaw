using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using Mapster;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.ProductAggregate;

namespace wfc.referential.Application.Products.Commands.PatchProduct;

public class PatchProductCommandHandler(IProductRepository _ProductRepository, ICacheService cacheService) 
    : ICommandHandler<PatchProductCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(PatchProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _ProductRepository.GetByIdAsync(request.ProductId, cancellationToken);

        if (product is null)
            throw new ResourceNotFoundException($"{nameof(Product)} not found");

        request.Adapt(product);
        product.Patch();
        await _ProductRepository.UpdateProductAsync(product, cancellationToken);

        await cacheService.SetAsync(request.CacheKey, product, TimeSpan.FromMinutes(request.CacheExpiration), cancellationToken);

        return Result.Success(product.Id!.Value);
    }
}
