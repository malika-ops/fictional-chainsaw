using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.ProductAggregate;
using wfc.referential.Domain.ProductAggregate.Exceptions;

namespace wfc.referential.Application.Products.Commands.CreateProduct;

public class CreateProductCommandHandler(IProductRepository ProductRepository, ICacheService cacheService) 
    : ICommandHandler<CreateProductCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {

        var isExist = await ProductRepository.GetByCodeAsync(request.Code,cancellationToken);
        if (isExist is not null) throw new CodeAlreadyExistException(request.Code);

        var product = Product.Create(ProductId.Of(Guid.NewGuid()), request.Code, request.Name, request.IsEnabled);

        await ProductRepository.AddProductAsync(product, cancellationToken);

        await cacheService.SetAsync(request.CacheKey, product, TimeSpan.FromMinutes(request.CacheExpiration), cancellationToken);

        return Result.Success(product.Id!.Value);
    }
}
