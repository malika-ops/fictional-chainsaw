using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Constants;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.ProductAggregate;
using wfc.referential.Domain.ProductAggregate.Exceptions;

namespace wfc.referential.Application.Products.Commands.UpdateProduct;

public class PutProductCommandHandler(IProductRepository _productRepository, ICacheService cacheService)
    : ICommandHandler<UpdateProductCommand, Result<bool>>
{

    public async Task<Result<bool>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var productId = ProductId.Of(request.ProductId);
        var product = await _productRepository.GetOneByConditionAsync(p => p.Id == productId, cancellationToken);
        if (product is null) throw new ResourceNotFoundException("Product not found.");

        var hasDuplicatedCode = await _productRepository.GetOneByConditionAsync(p => p.Code.Equals(request.Code), cancellationToken);
        if (hasDuplicatedCode is not null) throw new CodeAlreadyExistException(request.Code);


        product.Update(request.Code, request.Name, request.IsEnabled);

        _productRepository.Update(product);
        await _productRepository.SaveChangesAsync(cancellationToken);

        await cacheService.RemoveByPrefixAsync(CacheKeys.ProductCache.Prefix, cancellationToken);

        return Result.Success(true);
    }
}
