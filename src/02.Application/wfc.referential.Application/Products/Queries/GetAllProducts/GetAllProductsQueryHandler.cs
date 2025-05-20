using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using Mapster;
using wfc.referential.Application.Interfaces;
using wfc.referential.Application.Products.Dtos;

namespace wfc.referential.Application.Products.Queries.GetAllProducts;

public class GetAllProductsQueryHandler(IProductRepository productRepository, ICacheService cacheService)
    : IQueryHandler<GetAllProductsQuery, PagedResult<GetAllProductsResponse>>
{
    private readonly IProductRepository _productRepository = productRepository;
    private readonly ICacheService _cacheService = cacheService;

    public async Task<PagedResult<GetAllProductsResponse>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        var cachedProducts = await _cacheService.GetAsync<PagedResult<GetAllProductsResponse>>(request.CacheKey, cancellationToken);
        if (cachedProducts is not null)
        {
            return cachedProducts;
        }

        var products = await _productRepository
        .GetProductsByCriteriaAsync(request, cancellationToken);

        int totalCount = await _productRepository
            .GetCountTotalAsync(request, cancellationToken);

        var ProductsResponse = products.Adapt<List<GetAllProductsResponse>>();

        var result = new PagedResult<GetAllProductsResponse>(ProductsResponse, totalCount, request.PageNumber, request.PageSize);

        await _cacheService.SetAsync(request.CacheKey, 
            result, 
            TimeSpan.FromMinutes(request.CacheExpiration), 
            cancellationToken);

        return result;
    }
}
