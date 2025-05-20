using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.TypeDefinitionAggregate.Events;

public record TypeDefinitionCreatedEvent : IDomainEvent
{
    public Guid TypeDefinitionId { get; }
    public string Libelle { get; }
    public string Description { get; }
    public bool IsEnabled { get; }
    public DateTime OccurredOn { get; }

    public TypeDefinitionCreatedEvent(
        Guid typeDefinitionId,
        string libelle,
        string description,
        bool isEnabled,
        DateTime occurredOn)
    {
        TypeDefinitionId = typeDefinitionId;
        Libelle = libelle;
        Description = description;
        IsEnabled = isEnabled;
        OccurredOn = occurredOn;
    }
}