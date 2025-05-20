using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.SupportAccountAggregate.Events;

public record SupportAccountBalanceUpdatedEvent : IDomainEvent
{
    public Guid SupportAccountId { get; }
    public decimal OldBalance { get; }
    public decimal NewBalance { get; }
    public DateTime OccurredOn { get; }

    public SupportAccountBalanceUpdatedEvent(
        Guid supportAccountId,
        decimal oldBalance,
        decimal newBalance,
        DateTime occurredOn)
    {
        SupportAccountId = supportAccountId;
        OldBalance = oldBalance;
        NewBalance = newBalance;
        OccurredOn = occurredOn;
    }
}