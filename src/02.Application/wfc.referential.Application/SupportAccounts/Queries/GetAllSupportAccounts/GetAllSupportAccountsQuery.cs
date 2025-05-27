using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using wfc.referential.Application.SupportAccounts.Dtos;

namespace wfc.referential.Application.SupportAccounts.Queries.GetAllSupportAccounts;

public record GetAllSupportAccountsQuery : IQuery<PagedResult<GetSupportAccountsResponse>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? Code { get; init; }
    public string? Description { get; init; }
    public string? AccountingNumber { get; init; }
    public Guid? PartnerId { get; init; }
    public Guid? SupportAccountTypeId { get; init; }
    public bool? IsEnabled { get; init; } = true;
}