using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Domain.IdentityDocumentAggregate.Events;

namespace wfc.referential.Domain.IdentityDocumentAggregate;

public class IdentityDocument : Aggregate<IdentityDocumentId>
{
    public string Code { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public bool IsEnabled { get; private set; } = true;

    private IdentityDocument() { }

    private IdentityDocument(IdentityDocumentId id, string code, string name, string? description, bool isEnabled)
    {
        Id = id;
        Code = code;
        Name = name;
        Description = description;
        IsEnabled = isEnabled;
    }

    public static IdentityDocument Create(IdentityDocumentId id, string code, string name, string? description)
    {
        var entity = new IdentityDocument
        {
            Id = id,
            Code = code,
            Name = name,
            Description = description,
            IsEnabled = true
        };

        entity.AddDomainEvent(new IdentityDocumentCreatedEvent(
            id.Value, code, name, description,true, DateTime.UtcNow));

        return entity;
    }

    public void Update(string code, string name, string description, bool isEnabled)
    {
        Code = code;
        Name = name;
        Description = description;
        IsEnabled = isEnabled;

        AddDomainEvent(new IdentityDocumentUpdatedEvent(
            Id.Value, Code, Name, Description, IsEnabled, DateTime.UtcNow));
    }

    public void SetInactive()
    {
        IsEnabled = false;
        AddDomainEvent(new IdentityDocumentStatusChangedEvent(
            Id!.Value, IsEnabled, DateTime.UtcNow));
    }

    public void Patch(string? code, string? name, string? description, bool? isEnabled)
    {
        Code = code ?? Code;
        Name = name ?? Name;
        Description = description ?? Description;
        IsEnabled = isEnabled ?? IsEnabled;

        AddDomainEvent(new IdentityDocumentPatchedEvent(
            Id.Value, Code, Name, Description, IsEnabled, DateTime.UtcNow));
    }
}

