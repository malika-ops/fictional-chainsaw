using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using Mapster;
using wfc.referential.Application.Interfaces;
using wfc.referential.Application.Products.Dtos;

namespace wfc.referential.Application.Products.Queries.GetFiltredProducts;

public class GetFiltredProductsQueryHandler(IProductRepository productRepository)
    : IQueryHandler<GetFiltredProductsQuery, PagedResult<GetProdcutsResponse>>
{
    private readonly IProductRepository _productRepository = productRepository;

    public async Task<PagedResult<GetProdcutsResponse>> Handle(GetFiltredProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await _productRepository.GetPagedByCriteriaAsync(request, request.PageNumber, request.PageSize, cancellationToken);
        var result = new PagedResult<GetProdcutsResponse>(
            products.Items.Adapt<List<GetProdcutsResponse>>(),
            products.TotalCount, request.PageNumber, request.PageSize);
        return result;
    }
}
