using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.ControleAggregate.Events;

public class ControleDisabledEvent : IDomainEvent
{
    public Guid ControleId { get; }
    public DateTime OccurredOn { get; }

    public ControleDisabledEvent(Guid controleId, DateTime occurredOn)
    {
        ControleId = controleId;
        OccurredOn = occurredOn;
    }
}