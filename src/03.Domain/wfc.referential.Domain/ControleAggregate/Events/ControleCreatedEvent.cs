using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.ControleAggregate.Events;

public class ControleCreatedEvent : IDomainEvent
{
    public Guid ControleId { get; }
    public string Code { get; }
    public string Name { get; }
    public DateTime OccurredOn { get; }

    public ControleCreatedEvent(Guid controleId, string code, string name, DateTime occurredOn)
    {
        ControleId = controleId;
        Code = code;
        Name = name;
        OccurredOn = occurredOn;
    }
}