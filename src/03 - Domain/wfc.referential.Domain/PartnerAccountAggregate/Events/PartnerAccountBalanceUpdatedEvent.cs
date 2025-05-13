using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.PartnerAccountAggregate.Events;

public record PartnerAccountBalanceUpdatedEvent : IDomainEvent
{
    public Guid PartnerAccountId { get; }
    public decimal OldBalance { get; }
    public decimal NewBalance { get; }
    public DateTime OccurredOn { get; }

    public PartnerAccountBalanceUpdatedEvent(
        Guid partnerAccountId,
        decimal oldBalance,
        decimal newBalance,
        DateTime occurredOn)
    {
        PartnerAccountId = partnerAccountId;
        OldBalance = oldBalance;
        NewBalance = newBalance;
        OccurredOn = occurredOn;
    }
}