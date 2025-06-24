using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using Mapster;
using wfc.referential.Application.Interfaces;
using wfc.referential.Application.Pricings.Dtos;

namespace wfc.referential.Application.Pricings.Queries.GetFiltredPricings;

public class GetFiltredPricingsQueryHandler : IQueryHandler<GetFiltredPricingsQuery, PagedResult<PricingResponse>>
{
    private readonly IPricingRepository _pricingRepo;

    public GetFiltredPricingsQueryHandler(IPricingRepository pricingRepo)
    {
        _pricingRepo = pricingRepo;
    }

    public async Task<PagedResult<PricingResponse>> Handle(
        GetFiltredPricingsQuery query, CancellationToken ct)
    {
        var paged = await _pricingRepo.GetPagedByCriteriaAsync(
            query,
            query.PageNumber,
            query.PageSize,
            ct,
            p=>p.Corridor,p=>p.Service,p=>p.Affiliate);

        return new PagedResult<PricingResponse>(
            paged.Items.Adapt<List<PricingResponse>>(),
            paged.TotalCount,
            paged.PageNumber,
            paged.PageSize);
    }
}
