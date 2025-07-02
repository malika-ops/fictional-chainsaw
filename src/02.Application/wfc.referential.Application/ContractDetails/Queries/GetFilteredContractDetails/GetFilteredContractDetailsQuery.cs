using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using wfc.referential.Application.ContractDetails.Dtos;

namespace wfc.referential.Application.ContractDetails.Queries.GetFilteredContractDetails;

public record GetFilteredContractDetailsQuery : IQuery<PagedResult<GetContractDetailsResponse>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public Guid? ContractId { get; init; }
    public Guid? PricingId { get; init; }
    public bool? IsEnabled { get; init; } = true;
}