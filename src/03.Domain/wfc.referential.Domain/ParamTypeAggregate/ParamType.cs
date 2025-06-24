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
            paramType.TypeDefinitionId.Value,
            paramType.Value,
            paramType.IsEnabled,
            DateTime.UtcNow
        ));

        return paramType;
    }

    public void Update(
        string value,
        bool? isEnabled)
    {
        Value = value;
        IsEnabled = isEnabled ?? IsEnabled;

        AddDomainEvent(new ParamTypeUpdatedEvent(
            Id.Value,
            TypeDefinitionId.Value,
            Value,
            IsEnabled,
            DateTime.UtcNow
        ));
    }

    public void Patch(
        string? value,
        bool? isEnabled)
    {
        Value = value ?? Value;
        IsEnabled = isEnabled ?? IsEnabled;

        AddDomainEvent(new ParamTypePatchedEvent(
            Id.Value,
            TypeDefinitionId.Value,
            Value,
            IsEnabled,
            DateTime.UtcNow
        ));
    }

    public void Disable()
    {
        IsEnabled = false;

        AddDomainEvent(new ParamTypeDisabledEvent(
            Id.Value,
            DateTime.UtcNow
        ));
    }

    public void Activate()
    {
        IsEnabled = true;

        AddDomainEvent(new ParamTypeActivatedEvent(
            Id.Value,
            DateTime.UtcNow
        ));
    }
}