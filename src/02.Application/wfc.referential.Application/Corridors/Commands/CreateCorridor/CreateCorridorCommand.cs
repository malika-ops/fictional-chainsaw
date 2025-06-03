using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.Corridors.Commands.CreateCorridor;

public record CreateCorridorCommand : ICommand<Result<Guid>>
{
    public Guid? SourceCountryId { get; init; } = default!;
    public Guid? DestinationCountryId { get; init; } = default!;
    public Guid? SourceCityId { get; init; } = default!;
    public Guid? DestinationCityId { get; init; } = default!;
    public Guid? SourceBranchId { get; init; } = default!;
    public Guid? DestinationBranchId { get; init; } = default!;

}