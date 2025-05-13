using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Caching.Interface;
using wfc.referential.Domain.AgencyAggregate;
using wfc.referential.Domain.CityAggregate;
using wfc.referential.Domain.CorridorAggregate;
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.RegionAggregate;

namespace wfc.referential.Application.Corridors.Commands.PatchCorridor;

public record PatchCorridorCommand : ICommand<Result<Guid>>, ICacheableQuery
{
    public Guid CorridorId { get; set; } = default!;
    public CountryId? SourceCountryId { get; init; } = default!;
    public CountryId? DestinationCountryId { get; init; } = default!;
    public CityId? SourceCityId { get; init; } = default!;
    public CityId? DestinationCityId { get; init; } = default!;
    public AgencyId? SourceAgencyId { get; init; } = default!;
    public AgencyId? DestinationAgencyId { get; init; } = default!;
    public bool? IsEnabled { get; init; } = default!;

    public string CacheKey => $"{nameof(Corridor)}_{CorridorId}";
    public int CacheExpiration => 5;
}
