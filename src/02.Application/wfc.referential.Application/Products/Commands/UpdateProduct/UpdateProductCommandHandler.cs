using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.ProductAggregate.Exceptions;

namespace wfc.referential.Application.Products.Commands.UpdateProduct;

public class PutProductCommandHandler(IProductRepository _ProductRepository, ICacheService cacheService)
    : ICommandHandler<UpdateProductCommand, Result<Guid>>
{

    public async Task<Result<Guid>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var Product = await _ProductRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (Product is null) throw new ResourceNotFoundException("Product not found.");

        var hasDuplicatedCode = await _ProductRepository.GetByCodeAsync(request.Code, cancellationToken);
        if (hasDuplicatedCode is not null) throw new CodeAlreadyExistException(request.Code);

        Product.Update(request.Code, request.Name, request.IsEnabled);

        await _ProductRepository.UpdateProductAsync(Product, cancellationToken);

        await cacheService.SetAsync(request.CacheKey, Product, TimeSpan.FromMinutes(request.CacheExpiration), cancellationToken);

        return Result.Success(Product.Id!.Value);
    }
}
