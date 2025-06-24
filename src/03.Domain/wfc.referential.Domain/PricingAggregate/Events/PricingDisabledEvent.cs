using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.PricingAggregate.Events;

public record PricingDisabledEvent(Guid AgencyTierId, DateTime OccurredOn) : IDomainEvent;