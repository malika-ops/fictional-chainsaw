using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.IdentityDocumentAggregate.Events;

public record IdentityDocumentDisabledEvent(
    Guid IdentityDocumentId,
    DateTime OccurredOn) : IDomainEvent;