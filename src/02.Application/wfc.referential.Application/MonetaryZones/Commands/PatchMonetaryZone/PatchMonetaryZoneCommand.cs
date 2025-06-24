using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.MonetaryZones.Commands.PatchMonetaryZone;

public record PatchMonetaryZoneCommand : ICommand<Result<bool>>
{
    public Guid MonetaryZoneId { get; init; }
    public string? Code { get; init; }
    public string? Name { get; init; }
    public string? Description { get; init; }
    public bool? IsEnabled { get; init; }
}
