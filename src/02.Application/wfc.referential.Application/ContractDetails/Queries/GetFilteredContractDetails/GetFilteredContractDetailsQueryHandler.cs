using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using Mapster;
using wfc.referential.Application.Interfaces;
using wfc.referential.Application.ContractDetails.Dtos;

namespace wfc.referential.Application.ContractDetails.Queries.GetFilteredContractDetails;

public class GetFilteredContractDetailsQueryHandler : IQueryHandler<GetFilteredContractDetailsQuery, PagedResult<GetContractDetailsResponse>>
{
    private readonly IContractDetailsRepository _repo;

    public GetFilteredContractDetailsQueryHandler(IContractDetailsRepository repo) => _repo = repo;

    public async Task<PagedResult<GetContractDetailsResponse>> Handle(
        GetFilteredContractDetailsQuery contractDetailsQuery, CancellationToken ct)
    {
        var contractDetails = await _repo.GetPagedByCriteriaAsync(
            contractDetailsQuery,
            contractDetailsQuery.PageNumber,
            contractDetailsQuery.PageSize,
            ct);

        return new PagedResult<GetContractDetailsResponse>(
            contractDetails.Items.Adapt<List<GetContractDetailsResponse>>(),
            contractDetails.TotalCount,
            contractDetails.PageNumber,
            contractDetails.PageSize);
    }
}