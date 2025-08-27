using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using wfc.referential.Application.PartnerAccounts.Dtos;
using wfc.referential.Domain.PartnerAccountAggregate;

namespace wfc.referential.Application.PartnerAccounts.Queries.GetFiltredPartnerAccounts;

public record GetFiltredPartnerAccountsQuery : IQuery<PagedResult<PartnerAccountResponse>>
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
    public PartnerAccountTypeEnum? PartnerAccountType { get; init; }
    public bool? IsEnabled { get; init; } = true;
}