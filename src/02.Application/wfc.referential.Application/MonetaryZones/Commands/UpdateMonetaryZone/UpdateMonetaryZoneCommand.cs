using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.MonetaryZones.Commands.UpdateMonetaryZone;

public record UpdateMonetaryZoneCommand : ICommand<Result<bool>>
{
    public Guid MonetaryZoneId { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public bool IsEnabled { get; init; } = true;
}
