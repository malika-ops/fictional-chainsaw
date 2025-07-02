using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Domain.ControleAggregate.Events;
using wfc.referential.Domain.ControleAggregate.Exceptions;

namespace wfc.referential.Domain.ControleAggregate;

public class Controle : Aggregate<ControleId>
{
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public bool IsEnabled { get; private set; } = true;

    private Controle() { }

    public static Controle Create(ControleId id, string code, string name)
    {
        var entity = new Controle
        {
            Id = id,
            Code = code,
            Name = name
        };

        entity.AddDomainEvent(new ControleCreatedEvent(
            entity.Id.Value,
            entity.Code,
            entity.Name,
            DateTime.UtcNow
        ));

        return entity;
    }

    public void Update(string code, string name, bool isEnabled)
    {
        Code = code;
        Name = name;
        IsEnabled = isEnabled;

        AddDomainEvent(new ControleUpdatedEvent(
            Id!.Value,
            Code,
            Name,
            IsEnabled,
            DateTime.UtcNow
        ));
    }

    public void Patch(string? code, string? name, bool? isEnabled)
    {
        Code = code ?? Code;
        Name = name ?? Name;
        IsEnabled = isEnabled ?? IsEnabled;

        AddDomainEvent(new ControlePatchedEvent(
            Id!.Value,
            Code,
            Name,
            IsEnabled,
            DateTime.UtcNow
        ));
    }

    public void Disable()
    {
        IsEnabled = false;
        AddDomainEvent(new ControleDisabledEvent(
            Id.Value,
            DateTime.UtcNow
        ));
    }
}