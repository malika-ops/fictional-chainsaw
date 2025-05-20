using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.BankAggregate.Events;

public record BankDisabledEvent : IDomainEvent
{
    public Guid BankId { get; }
    public DateTime OccurredOn { get; }

    public BankDisabledEvent(
        Guid bankId,
        DateTime occurredOn)
    {
        BankId = bankId;
        OccurredOn = occurredOn;
    }
}
