using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using Mapster;
using wfc.referential.Application.Interfaces;
using wfc.referential.Application.Taxes.Dtos;

namespace wfc.referential.Application.Taxes.Queries.GetAllTaxes;

public class GetAllTaxesQueryHandler(ITaxRepository _taxRepository)
    : IQueryHandler<GetAllTaxesQuery, PagedResult<GetAllTaxesResponse>>
{
    public async Task<PagedResult<GetAllTaxesResponse>> Handle(GetAllTaxesQuery request, CancellationToken cancellationToken)
    {

        var taxes = await _taxRepository.GetPagedByCriteriaAsync(request, request.PageNumber, request.PageSize, cancellationToken);
        var result = new PagedResult<GetAllTaxesResponse>(
            taxes.Items.Adapt<List<GetAllTaxesResponse>>(),
            taxes.TotalCount, request.PageNumber, request.PageSize);
        return result;

    }
}
