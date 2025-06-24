using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using Mapster;
using wfc.referential.Application.Interfaces;
using wfc.referential.Application.Contracts.Dtos;

namespace wfc.referential.Application.Contracts.Queries.GetFiltredContracts;

public class GetFiltredContractsQueryHandler : IQueryHandler<GetFiltredContractsQuery, PagedResult<GetContractsResponse>>
{
    private readonly IContractRepository _repo;

    public GetFiltredContractsQueryHandler(IContractRepository repo) => _repo = repo;

    public async Task<PagedResult<GetContractsResponse>> Handle(
        GetFiltredContractsQuery contractQuery, CancellationToken ct)
    {
        var contracts = await _repo.GetPagedByCriteriaAsync(contractQuery, contractQuery.PageNumber, contractQuery.PageSize, ct);
        return new PagedResult<GetContractsResponse>(contracts.Items.Adapt<List<GetContractsResponse>>(), contracts.TotalCount, contracts.PageNumber, contracts.PageSize);
    }
}