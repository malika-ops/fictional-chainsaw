using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.IdentityDocumentAggregate.Events;
public record IdentityDocumentCreatedEvent(
    Guid IdentityDocumentId,
    string Code,
    string Name,
    string? Description,
    bool IsEnabled,
    DateTime OccurredOn
) : IDomainEvent;