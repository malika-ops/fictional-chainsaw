using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.TypeDefinitionAggregate.Events;

public record TypeDefinitionPatchedEvent : IDomainEvent
{
    public Guid TypeDefinitionId { get; }
    public string Libelle { get; }
    public string Description { get; }
    public bool IsEnabled { get; }
    public DateTime OccurredOn { get; }

    public TypeDefinitionPatchedEvent(
        Guid typeDefinitionId,
        string libelle,
        string description,
        DateTime occurredOn,
        bool isEnabled)
    {
        TypeDefinitionId = typeDefinitionId;
        Libelle = libelle;
        Description = description;
        OccurredOn = occurredOn;
        IsEnabled = isEnabled;
    }
}