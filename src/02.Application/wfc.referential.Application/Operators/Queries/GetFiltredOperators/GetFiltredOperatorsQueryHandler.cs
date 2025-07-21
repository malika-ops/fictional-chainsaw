using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using Mapster;
using wfc.referential.Application.Interfaces;
using wfc.referential.Application.Operators.Dtos;

namespace wfc.referential.Application.Operators.Queries.GetFiltredOperators;

public class GetFiltredOperatorsQueryHandler : IQueryHandler<GetFiltredOperatorsQuery, PagedResult<GetOperatorsResponse>>
{
    private readonly IOperatorRepository _repo;

    public GetFiltredOperatorsQueryHandler(IOperatorRepository repo) => _repo = repo;

    public async Task<PagedResult<GetOperatorsResponse>> Handle(
        GetFiltredOperatorsQuery operatorQuery, CancellationToken ct)
    {
        var operators = await _repo.GetPagedByCriteriaAsync(
            operatorQuery,
            operatorQuery.PageNumber,
            operatorQuery.PageSize,
            ct);

        return new PagedResult<GetOperatorsResponse>(
            operators.Items.Adapt<List<GetOperatorsResponse>>(),
            operators.TotalCount,
            operators.PageNumber,
            operators.PageSize);
    }
}