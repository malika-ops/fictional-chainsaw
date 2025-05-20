using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.IdentityDocumentAggregate.Events;
public record IdentityDocumentStatusChangedEvent(
    Guid IdentityDocumentId,
    bool IsEnabled,
    DateTime OccurredOn
) : IDomainEvent;