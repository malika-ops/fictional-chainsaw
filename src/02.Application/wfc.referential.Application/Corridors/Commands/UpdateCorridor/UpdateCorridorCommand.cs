using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.Corridors.Commands.UpdateCorridor;

public record UpdateCorridorCommand : ICommand<Result<bool>>
{
    public Guid CorridorId { get; set; } = default!;
    public Guid? SourceCountryId { get; init; } = default!;
    public Guid? DestinationCountryId { get; init; } = default!;
    public Guid? SourceCityId { get; init; } = default!;
    public Guid? DestinationCityId { get; init; } = default!;
    public Guid? SourceBranchId { get; init; } = default!;
    public Guid? DestinationBranchId { get; init; } = default!;
    public bool IsEnabled { get; init; } = default!;
}
