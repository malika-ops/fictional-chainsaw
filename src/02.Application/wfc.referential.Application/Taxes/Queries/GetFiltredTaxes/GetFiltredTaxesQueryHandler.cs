using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using Mapster;
using wfc.referential.Application.Interfaces;
using wfc.referential.Application.Taxes.Dtos;

namespace wfc.referential.Application.Taxes.Queries.GetFiltredTaxes;

public class GetFiltredTaxesQueryHandler(ITaxRepository _taxRepository)
    : IQueryHandler<GetFiltredTaxesQuery, PagedResult<GetTaxesResponse>>
{
    public async Task<PagedResult<GetTaxesResponse>> Handle(GetFiltredTaxesQuery request, CancellationToken cancellationToken)
    {

        var taxes = await _taxRepository.GetPagedByCriteriaAsync(request, request.PageNumber, request.PageSize, cancellationToken);
        var result = new PagedResult<GetTaxesResponse>(
            taxes.Items.Adapt<List<GetTaxesResponse>>(),
            taxes.TotalCount, request.PageNumber, request.PageSize);
        return result;

    }
}
