using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.MonetaryZones.Commands.DeleteMonetaryZone;

public record DeleteMonetaryZoneCommand : ICommand<Result<bool>>
{
    public Guid MonetaryZoneId { get; init; }
}
