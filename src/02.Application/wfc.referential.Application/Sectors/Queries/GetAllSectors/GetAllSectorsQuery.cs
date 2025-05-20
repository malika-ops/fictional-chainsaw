using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using wfc.referential.Application.Sectors.Dtos;

namespace wfc.referential.Application.Sectors.Queries.GetAllSectors;

public class GetAllSectorsQuery : IQuery<PagedResult<SectorResponse>>
{
    public int PageNumber { get; }
    public int PageSize { get; }
    public string? Code { get; init; }
    public string? Name { get; init; }
    public Guid? CityId { get; init; }
    public bool? IsEnabled { get; init; }

    public GetAllSectorsQuery(
        int pageNumber,
        int pageSize,
        string? code = null,
        string? name = null,
        Guid? cityId = null,
        bool? isEnabled = true)
    {
        Code = code;
        Name = name;
        CityId = cityId;
        PageNumber = pageNumber;
        PageSize = pageSize;
        IsEnabled = isEnabled;
    }
}