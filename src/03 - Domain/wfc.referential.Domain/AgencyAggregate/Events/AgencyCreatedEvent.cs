using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.AgencyAggregate.Events;

public record AgencyCreatedEvent(Guid AgencyId, string Code, string Label, DateTime OccurredOn) : IDomainEvent;
