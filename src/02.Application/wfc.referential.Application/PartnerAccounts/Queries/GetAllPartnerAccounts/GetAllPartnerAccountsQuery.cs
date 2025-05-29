using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using wfc.referential.Application.PartnerAccounts.Dtos;

namespace wfc.referential.Application.PartnerAccounts.Queries.GetAllPartnerAccounts;

public record GetAllPartnerAccountsQuery : IQuery<PagedResult<PartnerAccountResponse>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? AccountNumber { get; init; }
    public string? RIB { get; init; }
    public string? BusinessName { get; init; }
    public string? ShortName { get; init; }
    public decimal? MinAccountBalance { get; init; }
    public decimal? MaxAccountBalance { get; init; }
    public Guid? BankId { get; init; }
    public Guid? AccountTypeId { get; init; }
    public bool? IsEnabled { get; init; } = true;
}