using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using Mapster;
using wfc.referential.Application.Interfaces;
using wfc.referential.Application.TaxRuleDetails.Dtos;
using wfc.referential.Application.TaxRuleDetails.Queries.GetAllTaxeRuleDetails;

namespace wfc.referential.Application.TaxRuleDetails.Queries.GetAllTaxRuleDetails;

public class GetAllTaxRuleDetailsQueryHandler : IQueryHandler<GetAllTaxRuleDetailsQuery, PagedResult<GetAllTaxRuleDetailsResponse>>
{
    private readonly ITaxRuleDetailRepository _taxRuleDetailsRepository;

    public GetAllTaxRuleDetailsQueryHandler(
        ITaxRuleDetailRepository taxRuleDetailsRepository)
    {
        _taxRuleDetailsRepository = taxRuleDetailsRepository;
    }

    public async Task<PagedResult<GetAllTaxRuleDetailsResponse>> Handle(
        GetAllTaxRuleDetailsQuery request,
        CancellationToken cancellationToken)
    {

        var taxRuleDetails = await _taxRuleDetailsRepository
            .GetTaxRuleDetailsByCriteriaAsync(request, cancellationToken);

        int totalCount = await _taxRuleDetailsRepository
            .GetCountTotalAsync(request, cancellationToken);

        var response = taxRuleDetails.Adapt<List<GetAllTaxRuleDetailsResponse>>();

        var result = new PagedResult<GetAllTaxRuleDetailsResponse>(
            response,
            totalCount,
            request.PageNumber,
            request.PageSize);

        return result;
    }
}