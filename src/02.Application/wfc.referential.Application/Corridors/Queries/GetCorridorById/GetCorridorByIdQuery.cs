using BuildingBlocks.Core.Abstraction.CQRS;
using wfc.referential.Application.Corridors.Dtos;

namespace wfc.referential.Application.Corridors.Queries.GetCorridorById;

public record GetCorridorByIdQuery : IQuery<GetCorridorResponse>
{
    public Guid CorridorId { get; init; }
} 