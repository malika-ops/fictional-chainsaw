using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using wfc.referential.Application.Banks.Dtos;

namespace wfc.referential.Application.Banks.Queries.GetAllBanks;

public class GetAllBanksQuery : IQuery<PagedResult<BankResponse>>
{
    public int PageNumber { get; }
    public int PageSize { get; }
    public string? Code { get; init; }
    public string? Name { get; init; }
    public string? Abbreviation { get; init; }
    public bool? IsEnabled { get; init; }

    public GetAllBanksQuery(
        int pageNumber,
        int pageSize,
        string? code = null,
        string? name = null,
        string? abbreviation = null,
        bool? isEnabled = true)
    {
        Code = code;
        Name = name;
        Abbreviation = abbreviation;
        PageNumber = pageNumber;
        PageSize = pageSize;
        IsEnabled = isEnabled;
    }
}