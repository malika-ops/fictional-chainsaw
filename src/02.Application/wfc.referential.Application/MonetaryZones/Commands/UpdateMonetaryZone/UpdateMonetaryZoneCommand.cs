using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.MonetaryZones.Commands.UpdateMonetaryZone;

public record UpdateMonetaryZoneCommand(Guid MonetaryZoneId, string Code, string Name , string Description , bool IsEnabled = true) : ICommand<Result<Guid>>
{

}
