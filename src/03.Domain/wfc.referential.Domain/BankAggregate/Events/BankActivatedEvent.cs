using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.BankAggregate.Events;

public record BankActivatedEvent : IDomainEvent
{
    public Guid BankId { get; }
    public DateTime OccurredOn { get; }

    public BankActivatedEvent(
        Guid bankId,
        DateTime occurredOn)
    {
        BankId = bankId;
        OccurredOn = occurredOn;
    }
}