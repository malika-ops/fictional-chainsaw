using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.ProductAggregate;
using wfc.referential.Domain.ProductAggregate.Exceptions;

namespace wfc.referential.Application.Products.Commands.DeleteProduct;

public class DeleteProductCommandHandler(IProductRepository _ProductRepository, ICacheService cacheService) 
    : ICommandHandler<DeleteProductCommand, Result<bool>>
{
    
    public async Task<Result<bool>> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _ProductRepository.GetByIdAsync(ProductId.Of(request.ProductId).Value, cancellationToken);

        if (product is null)
            throw new ResourceNotFoundException($"{nameof(Product)} not found");

        var services = await _ProductRepository.GetServicesByProductIdAsync(product.Id!.Value, cancellationToken);

        if (services.Count > 0) throw new ProductHasServicesException(services);
        
        product.SetInactive();
        await _ProductRepository.UpdateProductAsync(product, cancellationToken);

        await cacheService.RemoveAsync(request.CacheKey, cancellationToken);

        return Result.Success(true);
    }
}