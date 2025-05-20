using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.MonetaryZones.Commands.DeleteMonetaryZone;

public record DeleteMonetaryZoneCommand(Guid MonetaryZoneId) : ICommand<Result<bool>>
{

}
