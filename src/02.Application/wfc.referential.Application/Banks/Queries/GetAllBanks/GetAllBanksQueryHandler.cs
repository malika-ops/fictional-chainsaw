using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using Mapster;
using wfc.referential.Application.Banks.Dtos;
using wfc.referential.Application.Interfaces;

namespace wfc.referential.Application.Banks.Queries.GetAllBanks;

public class GetAllBanksHandler
    : IQueryHandler<GetAllBanksQuery, PagedResult<GetBanksResponse>>
{
    private readonly IBankRepository _repo;
    public GetAllBanksHandler(IBankRepository repo) => _repo = repo;

    public async Task<PagedResult<GetBanksResponse>> Handle(
        GetAllBanksQuery bankQuery, CancellationToken ct)
    {
        var banks = await _repo.GetPagedByCriteriaAsync(bankQuery, bankQuery.PageNumber, bankQuery.PageSize, ct);
        return new PagedResult<GetBanksResponse>(banks.Items.Adapt<List<GetBanksResponse>>(), banks.TotalCount, banks.PageNumber, banks.PageSize);
    }
}