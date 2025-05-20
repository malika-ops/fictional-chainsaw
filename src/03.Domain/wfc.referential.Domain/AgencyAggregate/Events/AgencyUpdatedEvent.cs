using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.AgencyAggregate.Events;

public record AgencyUpdatedEvent(
    Guid AgencyId,
    string Code,
    string Name,
    DateTime OccurredOn) : IDomainEvent;