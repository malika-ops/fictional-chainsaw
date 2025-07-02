using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using Mapster;
using wfc.referential.Application.Interfaces;
using wfc.referential.Application.TaxRuleDetails.Dtos;
using wfc.referential.Application.TaxRuleDetails.Queries.GetFiltredTaxeRuleDetails;

namespace wfc.referential.Application.TaxRuleDetails.Queries.GetFiltredTaxRuleDetails;

public class GetFiltredTaxRuleDetailsQueryHandler(ITaxRuleDetailRepository _taxRuleDetailsRepository) : IQueryHandler<GetFiltredTaxRuleDetailsQuery, PagedResult<GetTaxRuleDetailsResponse>>
{

    public async Task<PagedResult<GetTaxRuleDetailsResponse>> Handle(
        GetFiltredTaxRuleDetailsQuery request,
        CancellationToken cancellationToken)
    {

        var paged = await _taxRuleDetailsRepository.GetPagedByCriteriaAsync(
           request,
           request.PageNumber,
           request.PageSize,
           cancellationToken);

        return new PagedResult<GetTaxRuleDetailsResponse>(
            paged.Items.Adapt<List<GetTaxRuleDetailsResponse>>(),
            paged.TotalCount,
            paged.PageNumber,
            paged.PageSize);

    }
}