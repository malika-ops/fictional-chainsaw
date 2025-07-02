using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.ControleAggregate.Events;

public class ControleUpdatedEvent : IDomainEvent
{
    public Guid ControleId { get; }
    public string Code { get; }
    public string Name { get; }
    public bool IsEnabled { get; }
    public DateTime OccurredOn { get; }

    public ControleUpdatedEvent(Guid controleId, string code, string name, bool isEnabled, DateTime occurredOn)
    {
        ControleId = controleId;
        Code = code;
        Name = name;
        IsEnabled = isEnabled;
        OccurredOn = occurredOn;
    }
}