using BuildingBlocks.Core.Abstraction.CQRS;
using wfc.referential.Application.MonetaryZones.Dtos;

namespace wfc.referential.Application.MonetaryZones.Queries.GetMonetaryZoneById;

public record GetMonetaryZoneByIdQuery : IQuery<MonetaryZoneResponse>
{
    public Guid MonetaryZoneId { get; init; }
} 