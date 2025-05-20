using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using wfc.referential.Application.Agencies.Dtos;

namespace wfc.referential.Application.Agencies.Queries.GetAllAgencies;

public record GetAllAgenciesQuery : IQuery<PagedResult<GetAgenciesResponse>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;

    public string? Code { get; init; }
    public string? Name { get; init; }
    public string? Abbreviation { get; init; }
    public string? Address { get; init; }
    public string? Phone { get; init; }
    public string? Fax { get; init; }
    public string? AccountingSheetName { get; init; }
    public string? AccountingAccountNumber { get; init; }
    public string? MoneyGramReferenceNumber { get; init; }
    public string? PostalCode { get; init; }

    public Guid? CityId { get; init; }
    public Guid? SectorId { get; init; }
    public Guid? AgencyTypeId { get; init; }
    public string? AgencyTypeValue { get; init; }
    public string? AgencyTypeLibelle { get; init; }

    public bool? IsEnabled { get; init; } = true;
}