using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using wfc.referential.Application.SupportAccounts.Dtos;

namespace wfc.referential.Application.SupportAccounts.Queries.GetAllSupportAccounts;

public class GetAllSupportAccountsQuery : IQuery<PagedResult<SupportAccountResponse>>
{
    public int PageNumber { get; }
    public int PageSize { get; }
    public string? Code { get; init; }
    public string? Name { get; init; }
    public decimal? MinThreshold { get; init; }
    public decimal? MaxThreshold { get; init; }
    public decimal? MinLimit { get; init; }
    public decimal? MaxLimit { get; init; }
    public decimal? MinAccountBalance { get; init; }
    public decimal? MaxAccountBalance { get; init; }
    public string? AccountingNumber { get; init; }
    public Guid? PartnerId { get; init; }
    public string? SupportAccountType { get; init; }
    public bool? IsEnabled { get; init; }

    public GetAllSupportAccountsQuery(
        int pageNumber,
        int pageSize,
        string? code = null,
        string? name = null,
        decimal? minThreshold = null,
        decimal? maxThreshold = null,
        decimal? minLimit = null,
        decimal? maxLimit = null,
        decimal? minAccountBalance = null,
        decimal? maxAccountBalance = null,
        string? accountingNumber = null,
        Guid? partnerId = null,
        string? supportAccountType = null,
        bool? isEnabled = null)
    {
        Code = code;
        Name = name;
        MinThreshold = minThreshold;
        MaxThreshold = maxThreshold;
        MinLimit = minLimit;
        MaxLimit = maxLimit;
        MinAccountBalance = minAccountBalance;
        MaxAccountBalance = maxAccountBalance;
        AccountingNumber = accountingNumber;
        PartnerId = partnerId;
        SupportAccountType = supportAccountType;
        PageNumber = pageNumber;
        PageSize = pageSize;
        IsEnabled = isEnabled;
    }
}