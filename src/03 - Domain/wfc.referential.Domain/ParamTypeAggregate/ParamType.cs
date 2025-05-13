using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Domain.ParamTypeAggregate.Events;
using wfc.referential.Domain.TypeDefinitionAggregate;

namespace wfc.referential.Domain.ParamTypeAggregate;
public class ParamType : Aggregate<ParamTypeId>
{
    public TypeDefinitionId TypeDefinitionId { get; private set; }
    public string Value { get; private set; } = string.Empty;
    public bool IsEnabled { get; private set; } = true;
    public TypeDefinition TypeDefinition { get; set; }

    private ParamType() { }

    public static ParamType Create(
    ParamTypeId paramTypeId,
    TypeDefinitionId typeDefinitionId,
    string value)
    {
        var paramType = new ParamType
        {
            Id = paramTypeId,
            TypeDefinitionId = typeDefinitionId,
            Value = value,
            IsEnabled = true
        };

        paramType.AddDomainEvent(new ParamTypeCreatedEvent(
            paramType.Id.Value,
            paramType.Value,
            paramType.IsEnabled,
            DateTime.UtcNow
        ));

        return paramType;
    }

    public void Update(string value)
    {
        Value = value;

        AddDomainEvent(new ParamTypeUpdatedEvent(
            Id.Value,
            Value
        ));
    }

    public void Patch()
    {
        AddDomainEvent(new ParamTypePatchedEvent(
            Id.Value,
            Value,
            DateTime.UtcNow
        ));
    }

    public void Disable()
    {
        IsEnabled = false;

        // raise the disable event
        AddDomainEvent(new ParamTypeDisabledEvent(
            Id.Value,
            DateTime.UtcNow
        ));
    }

    public void Activate()
    {
        IsEnabled = true;

        // raise the activate event
        AddDomainEvent(new ParamTypeActivatedEvent(
            Id.Value,
            DateTime.UtcNow
        ));
    }
}