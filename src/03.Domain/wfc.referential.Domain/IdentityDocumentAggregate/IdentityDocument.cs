using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Domain.IdentityDocumentAggregate.Events;

namespace wfc.referential.Domain.IdentityDocumentAggregate;

public class IdentityDocument : Aggregate<IdentityDocumentId>
{
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsEnabled { get; private set; } = true;

    private IdentityDocument() { }

    public static IdentityDocument Create(
        IdentityDocumentId id,
        string code,
        string name,
        string? description)
    {
        var identityDocument = new IdentityDocument
        {
            Id = id,
            Code = code,
            Name = name,
            Description = description,
            IsEnabled = true
        };

        identityDocument.AddDomainEvent(new IdentityDocumentCreatedEvent(
            identityDocument.Id.Value,
            identityDocument.Code,
            identityDocument.Name,
            identityDocument.Description,
            identityDocument.IsEnabled,
            DateTime.UtcNow));

        return identityDocument;
    }

    public void Update(
        string code,
        string name,
        string? description,
        bool? isEnabled)
    {
        Code = code;
        Name = name;
        Description = description;
        IsEnabled = isEnabled ?? IsEnabled;

        AddDomainEvent(new IdentityDocumentUpdatedEvent(
            Id.Value,
            Code,
            Name,
            Description,
            IsEnabled,
            DateTime.UtcNow));
    }

    public void Patch(
        string? code,
        string? name,
        string? description,
        bool? isEnabled)
    {
        Code = code ?? Code;
        Name = name ?? Name;
        Description = description ?? Description;
        IsEnabled = isEnabled ?? IsEnabled;

        AddDomainEvent(new IdentityDocumentPatchedEvent(
            Id.Value,
            Code,
            Name,
            Description,
            IsEnabled,
            DateTime.UtcNow));
    }

    public void Disable()
    {
        IsEnabled = false;

        AddDomainEvent(new IdentityDocumentDisabledEvent(
            Id.Value,
            DateTime.UtcNow));
    }

    public void Activate()
    {
        IsEnabled = true;

        AddDomainEvent(new IdentityDocumentActivatedEvent(
            Id.Value,
            DateTime.UtcNow));
    }
}