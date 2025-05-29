using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using Mapster;
using wfc.referential.Application.Interfaces;
using wfc.referential.Application.PartnerAccounts.Dtos;

namespace wfc.referential.Application.PartnerAccounts.Queries.GetAllPartnerAccounts;

public class GetAllPartnerAccountsQueryHandler : IQueryHandler<GetAllPartnerAccountsQuery, PagedResult<PartnerAccountResponse>>
{
    private readonly IPartnerAccountRepository _repo;

    public GetAllPartnerAccountsQueryHandler(IPartnerAccountRepository repo) => _repo = repo;

    public async Task<PagedResult<PartnerAccountResponse>> Handle(GetAllPartnerAccountsQuery query, CancellationToken ct)
    {
        var partnerAccounts = await _repo.GetPagedByCriteriaAsync(query, query.PageNumber, query.PageSize, ct);
        return new PagedResult<PartnerAccountResponse>(
            partnerAccounts.Items.Adapt<List<PartnerAccountResponse>>(),
            partnerAccounts.TotalCount,
            partnerAccounts.PageNumber,
            partnerAccounts.PageSize);
    }
}