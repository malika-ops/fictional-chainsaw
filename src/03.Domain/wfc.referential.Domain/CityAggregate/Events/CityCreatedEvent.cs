using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Domain.RegionAggregate;

namespace wfc.referential.Domain.CityAggregate.Events;
public record CityCreatedEvent(
    Guid CityId, string CityCode, string CityName, string Abbreviation,
    string TimeZone, bool Status, RegionId RegionId, DateTime OccurredOn) : IDomainEvent;