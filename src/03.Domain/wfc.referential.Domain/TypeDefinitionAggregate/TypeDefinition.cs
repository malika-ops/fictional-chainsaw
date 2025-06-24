using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Domain.ParamTypeAggregate;
using wfc.referential.Domain.TypeDefinitionAggregate.Events;

namespace wfc.referential.Domain.TypeDefinitionAggregate;

public class TypeDefinition : Aggregate<TypeDefinitionId>
{
    public string Libelle { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public bool IsEnabled { get; private set; } = true;
    public List<ParamType> ParamTypes { get; private set; } = [];

    private TypeDefinition() { }

    public static TypeDefinition Create(
        TypeDefinitionId typeDefinitionId,
        string libelle,
        string description)
    {
        var typeDefinition = new TypeDefinition
        {
            Id = typeDefinitionId,
            Libelle = libelle,
            Description = description,
            IsEnabled = true
        };

        typeDefinition.AddDomainEvent(new TypeDefinitionCreatedEvent(
            typeDefinition.Id.Value,
            typeDefinition.Libelle,
            typeDefinition.Description,
            typeDefinition.IsEnabled,
            DateTime.UtcNow
        ));

        return typeDefinition;
    }

    public void Update(
        string libelle,
        string description,
        bool? isEnabled)
    {
        Libelle = libelle;
        Description = description;
        IsEnabled = isEnabled ?? IsEnabled;

        AddDomainEvent(new TypeDefinitionUpdatedEvent(
            Id.Value,
            Libelle,
            Description,
            IsEnabled,
            DateTime.UtcNow
        ));
    }

    public void Patch(
        string? libelle,
        string? description,
        bool? isEnabled)
    {
        Libelle = libelle ?? Libelle;
        Description = description ?? Description;
        IsEnabled = isEnabled ?? IsEnabled;

        AddDomainEvent(new TypeDefinitionPatchedEvent(
            Id.Value,
            Libelle,
            Description,
            IsEnabled,
            DateTime.UtcNow
        ));
    }

    public void Disable()
    {
        IsEnabled = false;

        AddDomainEvent(new TypeDefinitionDisabledEvent(
            Id.Value,
            DateTime.UtcNow
        ));
    }

    public void Activate()
    {
        IsEnabled = true;

        AddDomainEvent(new TypeDefinitionActivatedEvent(
            Id.Value,
            DateTime.UtcNow
        ));
    }

    public void AddParamType(ParamType paramType)
    {
        ParamTypes.Add(paramType);
    }

    public void RemoveParamType(ParamType paramType)
    {
        ParamTypes.Remove(paramType);
    }
}