using BuildingBlocks.Core.Abstraction.CQRS;
using wfc.referential.Application.RegionManagement.Dtos;

namespace wfc.referential.Application.Regions.Queries.GetRegionById;

public record GetRegionByIdQuery : IQuery<GetRegionsResponse>
{
    public Guid RegionId { get; init; }
} 