using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using Mapster;
using wfc.referential.Application.Interfaces;
using wfc.referential.Application.SupportAccounts.Dtos;

namespace wfc.referential.Application.SupportAccounts.Queries.GetAllSupportAccounts;

public class GetAllSupportAccountsHandler
    : IQueryHandler<GetAllSupportAccountsQuery, PagedResult<GetSupportAccountsResponse>>
{
    private readonly ISupportAccountRepository _repo;

    public GetAllSupportAccountsHandler(ISupportAccountRepository repo) => _repo = repo;

    public async Task<PagedResult<GetSupportAccountsResponse>> Handle(
        GetAllSupportAccountsQuery supportAccountQuery, CancellationToken ct)
    {
        var supportAccounts = await _repo.GetPagedByCriteriaAsync(supportAccountQuery, supportAccountQuery.PageNumber, supportAccountQuery.PageSize, ct);
        return new PagedResult<GetSupportAccountsResponse>(supportAccounts.Items.Adapt<List<GetSupportAccountsResponse>>(), supportAccounts.TotalCount, supportAccounts.PageNumber, supportAccounts.PageSize);
    }
}