using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.IdentityDocumentAggregate.Events;
public record IdentityDocumentPatchedEvent(
    Guid IdentityDocumentId,
    string Code,
    string Name,
    string? Description,
    bool IsEnabled,
    DateTime OccurredOn
) : IDomainEvent;
