using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using wfc.referential.Application.Sectors.Dtos;

namespace wfc.referential.Application.Sectors.Queries.GetAllSectors;

public record GetAllSectorsQuery : IQuery<PagedResult<SectorResponse>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;

    public string? Code { get; init; }
    public string? Name { get; init; }
    public Guid? CityId { get; init; }
    public bool? IsEnabled { get; init; } = true;
}