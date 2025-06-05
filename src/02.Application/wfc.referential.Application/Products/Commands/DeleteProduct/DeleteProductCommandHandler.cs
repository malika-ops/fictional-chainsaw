using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Constants;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.ProductAggregate;
using wfc.referential.Domain.ProductAggregate.Exceptions;

namespace wfc.referential.Application.Products.Commands.DeleteProduct;

public class DeleteProductCommandHandler(IProductRepository _productRepository,IServiceRepository _serviceRepository,
    ICacheService cacheService) 
    : ICommandHandler<DeleteProductCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var productId = ProductId.Of(request.ProductId);
        var product = await _productRepository.GetOneByConditionAsync(p => p.Id == productId, cancellationToken);
        
        if (product is null)
            throw new ResourceNotFoundException($"{nameof(Product)} not found");

        var services = await _serviceRepository.GetByConditionAsync(s => s.ProductId == productId, cancellationToken);

        if (services.Any()) throw new ProductHasServicesException(services);
        
        product.SetInactive();
        _productRepository.Update(product);

        await cacheService.RemoveByPrefixAsync(CacheKeys.ProductCache.Prefix, cancellationToken);

        return Result.Success(true);
    }
}