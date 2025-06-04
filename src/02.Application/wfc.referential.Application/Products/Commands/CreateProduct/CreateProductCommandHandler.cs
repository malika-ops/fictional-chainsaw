using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Application.Constants;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.ProductAggregate;
using wfc.referential.Domain.ProductAggregate.Exceptions;

namespace wfc.referential.Application.Products.Commands.CreateProduct;

public class CreateProductCommandHandler(IProductRepository _productRepository, ICacheService cacheService) 
    : ICommandHandler<CreateProductCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {

        var isExist = await _productRepository.GetOneByConditionAsync(p => p.Code.Equals(request.Code),cancellationToken);
        if (isExist is not null) throw new CodeAlreadyExistException(request.Code);

        var product = Product.Create(ProductId.Of(Guid.NewGuid()), request.Code, request.Name, request.IsEnabled);

        await _productRepository.AddAsync(product, cancellationToken);
        await _productRepository.SaveChangesAsync(cancellationToken);

        await cacheService.RemoveByPrefixAsync(CacheKeys.ProductCache.Prefix, cancellationToken);

        return Result.Success(product.Id!.Value);
    }
}
