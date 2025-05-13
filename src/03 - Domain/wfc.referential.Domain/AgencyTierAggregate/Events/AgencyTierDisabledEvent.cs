using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.AgencyTierAggregate.Events;

public class AgencyTierDisabledEvent(Guid AgencyTierId, DateTime OccurredOn) : IDomainEvent;
