using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using Mapster;
using wfc.referential.Application.Interfaces;
using wfc.referential.Application.PartnerAccounts.Dtos;

namespace wfc.referential.Application.PartnerAccounts.Queries.GetFiltredPartnerAccounts;

public class GetFiltredPartnerAccountsQueryHandler : IQueryHandler<GetFiltredPartnerAccountsQuery, PagedResult<PartnerAccountResponse>>
{
    private readonly IPartnerAccountRepository _repo;

    public GetFiltredPartnerAccountsQueryHandler(IPartnerAccountRepository repo) => _repo = repo;

    public async Task<PagedResult<PartnerAccountResponse>> Handle(GetFiltredPartnerAccountsQuery query, CancellationToken ct)
    {
        var partnerAccounts = await _repo.GetPagedByCriteriaAsync(query, query.PageNumber, query.PageSize, ct);
        return new PagedResult<PartnerAccountResponse>(
            partnerAccounts.Items.Adapt<List<PartnerAccountResponse>>(),
            partnerAccounts.TotalCount,
            partnerAccounts.PageNumber,
            partnerAccounts.PageSize);
    }
}