using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.TypeDefinitionAggregate.Events;

public record TypeDefinitionDisabledEvent : IDomainEvent
{
    public Guid TypeDefinitionId { get; }
    public DateTime OccurredOn { get; }

    public TypeDefinitionDisabledEvent(
        Guid typeDefinitionId,
        DateTime occurredOn)
    {
        TypeDefinitionId = typeDefinitionId;
        OccurredOn = occurredOn;
    }
}

