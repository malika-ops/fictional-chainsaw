using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Domain.TierAggregate.Events;

namespace wfc.referential.Domain.TierAggregate;

public class Tier : Aggregate<TierId>
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public bool IsEnabled { get; private set; } = true;


    private Tier() { }

    public static Tier Create(TierId id, string name, string description, bool isEnabled = true)
    {
        var tier = new Tier
        {
            Id = id,
            Name = name,
            Description = description,
            IsEnabled = isEnabled
        };

        tier.AddDomainEvent(new TierCreatedEvent(id.Value, name, DateTime.UtcNow));
        return tier;
    }

    public void Update(string name, string description, bool isEnabled)
    {
        Name = name;
        Description = description;
        IsEnabled = isEnabled;

        AddDomainEvent(new TierUpdatedEvent(
            Id.Value,
            Name,
            Description,
            IsEnabled,
            DateTime.UtcNow));
    }

    public void Patch()
    {
        AddDomainEvent(new TierPatchedEvent(
            Id.Value,
            Name,
            Description,
            IsEnabled,
            DateTime.UtcNow));
    }

    public void Disable()
    {
        IsEnabled = false;
        AddDomainEvent(new TierDisabledEvent(Id.Value, DateTime.UtcNow));
    }
}
