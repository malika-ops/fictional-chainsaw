using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.TypeDefinitionAggregate.Events;

public record TypeDefinitionActivatedEvent : IDomainEvent
{
    public Guid TypeDefinitionId { get; }
    public DateTime OccurredOn { get; }

    public TypeDefinitionActivatedEvent(
        Guid typeDefinitionId,
        DateTime occurredOn)
    {
        TypeDefinitionId = typeDefinitionId;
        OccurredOn = occurredOn;
    }
}