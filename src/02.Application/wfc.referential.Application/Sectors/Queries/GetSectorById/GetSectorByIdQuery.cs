using BuildingBlocks.Core.Abstraction.CQRS;
using wfc.referential.Application.Sectors.Dtos;

namespace wfc.referential.Application.Sectors.Queries.GetSectorById;

public record GetSectorByIdQuery : IQuery<SectorResponse>
{
    public Guid SectorId { get; init; }
} 