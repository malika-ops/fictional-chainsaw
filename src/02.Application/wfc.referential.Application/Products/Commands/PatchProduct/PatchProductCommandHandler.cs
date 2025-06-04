using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using Mapster;
using wfc.referential.Application.Constants;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.CityAggregate.Exceptions;
using wfc.referential.Domain.ProductAggregate;

namespace wfc.referential.Application.Products.Commands.PatchProduct;

public class PatchProductCommandHandler(IProductRepository _productRepository, ICacheService cacheService) 
    : ICommandHandler<PatchProductCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(PatchProductCommand request, CancellationToken cancellationToken)
    {
        var productId = ProductId.Of(request.ProductId);
        var product = await _productRepository.GetOneByConditionAsync(p => p.Id == productId, cancellationToken);

        if (product is null)
            throw new ResourceNotFoundException($"{nameof(Product)} not found");

        var duplicatedCode = await _productRepository.GetOneByConditionAsync(p => p.Code.Equals(request.Code), cancellationToken);

        if (duplicatedCode is not null)
            throw new CodeAlreadyExistException($"{nameof(Product)} not found");

        product.Patch(request.Code, request.Name, request.IsEnabled);

        _productRepository.Update(product);
        await _productRepository.SaveChangesAsync(cancellationToken);

        await cacheService.RemoveByPrefixAsync(CacheKeys.ProductCache.Prefix, cancellationToken);

        return Result.Success(true);
    }
}
