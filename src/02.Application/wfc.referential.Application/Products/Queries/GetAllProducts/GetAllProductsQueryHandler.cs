using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using Mapster;
using wfc.referential.Application.Interfaces;
using wfc.referential.Application.Products.Dtos;

namespace wfc.referential.Application.Products.Queries.GetAllProducts;

public class GetAllProductsQueryHandler(IProductRepository productRepository)
    : IQueryHandler<GetAllProductsQuery, PagedResult<GetAllProductsResponse>>
{
    private readonly IProductRepository _productRepository = productRepository;

    public async Task<PagedResult<GetAllProductsResponse>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await _productRepository.GetPagedByCriteriaAsync(request, request.PageNumber, request.PageSize, cancellationToken);
        var result = new PagedResult<GetAllProductsResponse>(
            products.Items.Adapt<List<GetAllProductsResponse>>(),
            products.TotalCount, request.PageNumber, request.PageSize);
        return result;
    }
}
