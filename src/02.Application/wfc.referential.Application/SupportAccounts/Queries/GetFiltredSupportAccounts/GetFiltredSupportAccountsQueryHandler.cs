using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using Mapster;
using wfc.referential.Application.Interfaces;
using wfc.referential.Application.SupportAccounts.Dtos;

namespace wfc.referential.Application.SupportAccounts.Queries.GetFiltredSupportAccounts;

public class GetFiltredSupportAccountsHandler
    : IQueryHandler<GetFiltredSupportAccountsQuery, PagedResult<GetSupportAccountsResponse>>
{
    private readonly ISupportAccountRepository _repo;

    public GetFiltredSupportAccountsHandler(ISupportAccountRepository repo) => _repo = repo;

    public async Task<PagedResult<GetSupportAccountsResponse>> Handle(
        GetFiltredSupportAccountsQuery supportAccountQuery, CancellationToken ct)
    {
        var supportAccounts = await _repo.GetPagedByCriteriaAsync(supportAccountQuery, supportAccountQuery.PageNumber, supportAccountQuery.PageSize, ct);
        return new PagedResult<GetSupportAccountsResponse>(supportAccounts.Items.Adapt<List<GetSupportAccountsResponse>>(), supportAccounts.TotalCount, supportAccounts.PageNumber, supportAccounts.PageSize);
    }
}