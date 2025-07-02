using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Exceptions;
using Mapster;
using wfc.referential.Application.Products.Dtos;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.ProductAggregate;

namespace wfc.referential.Application.Products.Queries.GetProductById;

public class GetProductByIdQueryHandler : IQueryHandler<GetProductByIdQuery, GetProdcutsResponse>
{
    private readonly IProductRepository _productRepository;

    public GetProductByIdQueryHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<GetProdcutsResponse> Handle(GetProductByIdQuery query, CancellationToken ct)
    {
        var id = ProductId.Of(query.ProductId);
        var entity = await _productRepository.GetByIdAsync(id, ct)
            ?? throw new ResourceNotFoundException($"Product with id '{query.ProductId}' not found.");

        return entity.Adapt<GetProdcutsResponse>();
    }
} 