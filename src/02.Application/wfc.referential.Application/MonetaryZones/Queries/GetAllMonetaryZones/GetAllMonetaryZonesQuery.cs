using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using wfc.referential.Application.MonetaryZones.Dtos;
using wfc.referential.Domain.MonetaryZoneAggregate;

namespace wfc.referential.Application.MonetaryZones.Queries.GetAllMonetaryZones;

public class GetAllMonetaryZonesQuery : IQuery<PagedResult<MonetaryZoneResponse>>
{
    public int PageNumber { get; }
    public int PageSize { get; }
    public string? Code { get; init; }
    public string? Name { get; init; }
    public string? Description { get; init; }
    public bool? IsEnabled { get; init; }

    public GetAllMonetaryZonesQuery(int pageNumber, int pageSize, string? code,
        string? name,
        string? description,
        bool? isEnabled)
    {
        Code = code;
        Name = name;
        Description = description;
        PageNumber = pageNumber;
        PageSize = pageSize;
        IsEnabled = isEnabled;
    }
}
