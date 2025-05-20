using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.MonetaryZones.Commands.PatchMonetaryZone;

public record PatchMonetaryZoneCommand(Guid MonetaryZoneId, string? Code, string? Name, string? Description, bool? IsEnabled ) : ICommand<Result<Guid>>
{

}
