using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.MonetaryZones.Commands.CreateMonetaryZone;

public record CreateMonetaryZoneCommand(string Code, string Name, string Description) : ICommand<Result<Guid>>
{

}
