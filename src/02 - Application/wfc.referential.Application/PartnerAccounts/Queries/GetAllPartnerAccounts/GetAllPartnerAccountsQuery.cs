using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using wfc.referential.Application.PartnerAccounts.Dtos;

namespace wfc.referential.Application.PartnerAccounts.Queries.GetAllPartnerAccounts;

public class GetAllPartnerAccountsQuery : IQuery<PagedResult<PartnerAccountResponse>>
{
    public int PageNumber { get; }
    public int PageSize { get; }
    public string? AccountNumber { get; init; }
    public string? RIB { get; init; }
    public string? BusinessName { get; init; }
    public string? ShortName { get; init; }
    public decimal? MinAccountBalance { get; init; }
    public decimal? MaxAccountBalance { get; init; }
    public Guid? BankId { get; init; }
    public string? AccountType { get; init; }
    public bool? IsEnabled { get; init; }

    public GetAllPartnerAccountsQuery(
        int pageNumber,
        int pageSize,
        string? accountNumber = null,
        string? rib = null,
        string? businessName = null,
        string? shortName = null,
        decimal? minAccountBalance = null,
        decimal? maxAccountBalance = null,
        Guid? bankId = null,
        string? accountType = null,
        bool? isEnabled = null)
    {
        AccountNumber = accountNumber;
        RIB = rib;
        BusinessName = businessName;
        ShortName = shortName;
        MinAccountBalance = minAccountBalance;
        MaxAccountBalance = maxAccountBalance;
        BankId = bankId;
        AccountType = accountType;
        PageNumber = pageNumber;
        PageSize = pageSize;
        IsEnabled = isEnabled;
    }
}