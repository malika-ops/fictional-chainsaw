using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Caching.Interface;
using wfc.referential.Domain.AgencyAggregate;
using wfc.referential.Domain.CityAggregate;
using wfc.referential.Domain.CorridorAggregate;
using wfc.referential.Domain.Countries;

namespace wfc.referential.Application.Corridors.Commands.PatchCorridor;

public record PatchCorridorCommand : ICommand<Result<Guid>>, ICacheableQuery
{
    public Guid CorridorId { get; set; }
    public Guid? SourceCountryId { get; init; }
    public Guid? DestinationCountryId { get; init; }
    public Guid? SourceCityId { get; init; }
    public Guid? DestinationCityId { get; init; }
    public Guid? SourceAgencyId { get; init; }
    public Guid? DestinationAgencyId { get; init; }
    public bool? IsEnabled { get; init; }

    public string CacheKey => $"{nameof(Corridor)}_{CorridorId}";
    public int CacheExpiration => 5;
}
