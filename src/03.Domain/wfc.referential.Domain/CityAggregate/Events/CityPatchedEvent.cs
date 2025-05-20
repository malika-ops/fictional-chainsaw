using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Domain.RegionAggregate;

namespace wfc.referential.Domain.CityAggregate.Events;

public record CityPatchedEvent(
    Guid CityId, string CityCode, string CityName, string Abbreviation,
    string TimeZone, bool IsEnabled, RegionId RegionId, DateTime OccurredOn) : IDomainEvent;