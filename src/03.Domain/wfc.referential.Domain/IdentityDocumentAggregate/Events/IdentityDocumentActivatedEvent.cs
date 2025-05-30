using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.IdentityDocumentAggregate.Events;

public record IdentityDocumentActivatedEvent(
    Guid IdentityDocumentId,
    DateTime OccurredOn) : IDomainEvent;