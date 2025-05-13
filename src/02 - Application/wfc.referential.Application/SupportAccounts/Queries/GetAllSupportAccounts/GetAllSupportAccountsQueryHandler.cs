using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using Mapster;
using wfc.referential.Application.Interfaces;
using wfc.referential.Application.SupportAccounts.Dtos;

namespace wfc.referential.Application.SupportAccounts.Queries.GetAllSupportAccounts;

public class GetAllSupportAccountsQueryHandler : IQueryHandler<GetAllSupportAccountsQuery, PagedResult<SupportAccountResponse>>
{
    private readonly ISupportAccountRepository _supportAccountRepository;

    public GetAllSupportAccountsQueryHandler(ISupportAccountRepository supportAccountRepository)
    {
        _supportAccountRepository = supportAccountRepository;
    }

    public async Task<PagedResult<SupportAccountResponse>> Handle(GetAllSupportAccountsQuery request, CancellationToken cancellationToken)
    {
        var supportAccounts = await _supportAccountRepository
            .GetFilteredSupportAccountsAsync(request, cancellationToken);

        int totalCount = await _supportAccountRepository
            .GetCountTotalAsync(request, cancellationToken);

        var supportAccountResponses = supportAccounts.Adapt<List<SupportAccountResponse>>();

        return new PagedResult<SupportAccountResponse>(supportAccountResponses, totalCount, request.PageNumber, request.PageSize);
    }
}