using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.SupportAccountAggregate.Events;

public record SupportAccountUpdatedEvent : IDomainEvent
{
    public Guid SupportAccountId { get; }
    public string Code { get; }
    public string Name { get; }
    public decimal Threshold { get; }
    public decimal Limit { get; }
    public decimal AccountBalance { get; }
    public string AccountingNumber { get; }
    public Guid PartnerId { get; }
    public SupportAccountType SupportAccountType { get; }
    public bool IsEnabled { get; }
    public DateTime OccurredOn { get; }

    public SupportAccountUpdatedEvent(
        Guid supportAccountId,
        string code,
        string name,
        decimal threshold,
        decimal limit,
        decimal accountBalance,
        string accountingNumber,
        Guid partnerId,
        SupportAccountType supportAccountType,
        bool isEnabled,
        DateTime occurredOn)
    {
        SupportAccountId = supportAccountId;
        Code = code;
        Name = name;
        Threshold = threshold;
        Limit = limit;
        AccountBalance = accountBalance;
        AccountingNumber = accountingNumber;
        PartnerId = partnerId;
        SupportAccountType = supportAccountType;
        IsEnabled = isEnabled;
        OccurredOn = occurredOn;
    }
}