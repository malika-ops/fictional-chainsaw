using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using Mapster;
using wfc.referential.Application.Interfaces;
using wfc.referential.Application.PartnerAccounts.Dtos;

namespace wfc.referential.Application.PartnerAccounts.Queries.GetAllPartnerAccounts;

public class GetAllPartnerAccountsQueryHandler : IQueryHandler<GetAllPartnerAccountsQuery, PagedResult<PartnerAccountResponse>>
{
    private readonly IPartnerAccountRepository _partnerAccountRepository;

    public GetAllPartnerAccountsQueryHandler(IPartnerAccountRepository partnerAccountRepository)
    {
        _partnerAccountRepository = partnerAccountRepository;
    }

    public async Task<PagedResult<PartnerAccountResponse>> Handle(GetAllPartnerAccountsQuery request, CancellationToken cancellationToken)
    {
        var partnerAccounts = await _partnerAccountRepository
            .GetFilteredPartnerAccountsAsync(request, cancellationToken);

        int totalCount = await _partnerAccountRepository
            .GetCountTotalAsync(request, cancellationToken);

        var partnerAccountResponses = partnerAccounts.Adapt<List<PartnerAccountResponse>>();

        return new PagedResult<PartnerAccountResponse>(partnerAccountResponses, totalCount, request.PageNumber, request.PageSize);
    }
}