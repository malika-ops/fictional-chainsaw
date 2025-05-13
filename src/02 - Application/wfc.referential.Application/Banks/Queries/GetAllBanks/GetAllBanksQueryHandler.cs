using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using Mapster;
using wfc.referential.Application.Banks.Dtos;
using wfc.referential.Application.Interfaces;

namespace wfc.referential.Application.Banks.Queries.GetAllBanks;

public class GetAllBanksQueryHandler : IQueryHandler<GetAllBanksQuery, PagedResult<BankResponse>>
{
    private readonly IBankRepository _bankRepository;

    public GetAllBanksQueryHandler(IBankRepository bankRepository)
    {
        _bankRepository = bankRepository;
    }

    public async Task<PagedResult<BankResponse>> Handle(GetAllBanksQuery request, CancellationToken cancellationToken)
    {
        var banks = await _bankRepository
            .GetFilteredBanksAsync(request, cancellationToken);

        int totalCount = await _bankRepository
            .GetCountTotalAsync(request, cancellationToken);

        var bankResponses = banks.Adapt<List<BankResponse>>();

        return new PagedResult<BankResponse>(bankResponses, totalCount, request.PageNumber, request.PageSize);
    }
}