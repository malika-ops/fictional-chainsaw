using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Domain.CorridorAggregate;

namespace wfc.referential.Application.Corridors.Commands.PatchCorridor;

public record PatchCorridorCommand : ICommand<Result<bool>>
{
    public Guid CorridorId { get; set; }
    public Guid? SourceCountryId { get; init; }
    public Guid? DestinationCountryId { get; init; }
    public Guid? SourceCityId { get; init; }
    public Guid? DestinationCityId { get; init; }
    public Guid? SourceBranchId { get; init; }
    public Guid? DestinationBranchId { get; init; }
    public bool? IsEnabled { get; init; }
}
