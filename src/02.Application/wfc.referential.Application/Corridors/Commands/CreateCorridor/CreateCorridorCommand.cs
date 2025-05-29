using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Caching.Interface;
using wfc.referential.Domain.AgencyAggregate;
using wfc.referential.Domain.CityAggregate;
using wfc.referential.Domain.CorridorAggregate;
using wfc.referential.Domain.Countries;

namespace wfc.referential.Application.Corridors.Commands.CreateCorridor;

public record CreateCorridorCommand : ICommand<Result<Guid>>
{
    public CorridorId CorridorId { get; set; } = default!;
    public CountryId SourceCountryId { get; init; } = default!;
    public CountryId DestinationCountryId { get; init; } = default!;
    public CityId? SourceCityId { get; init; } = default!;
    public CityId? DestinationCityId { get; init; } = default!;
    public AgencyId? SourceBranchId { get; init; } = default!;
    public AgencyId? DestinationBranchId { get; init; } = default!;

}