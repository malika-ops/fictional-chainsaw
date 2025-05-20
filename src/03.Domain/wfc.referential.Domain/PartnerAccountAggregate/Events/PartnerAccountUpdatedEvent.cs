using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.PartnerAccountAggregate.Events;

public record PartnerAccountUpdatedEvent : IDomainEvent
{
    public Guid PartnerAccountId { get; }
    public string AccountNumber { get; }
    public string RIB { get; }
    public string Domiciliation { get; }
    public string BusinessName { get; }
    public string ShortName { get; }
    public decimal AccountBalance { get; }
    public Guid BankId { get; }
    public Guid AccountTypeId { get; }
    public bool IsEnabled { get; }
    public DateTime OccurredOn { get; }

    public PartnerAccountUpdatedEvent(
        Guid partnerAccountId,
        string accountNumber,
        string rib,
        string domiciliation,
        string businessName,
        string shortName,
        decimal accountBalance,
        Guid bankId,
        Guid accountTypeId,
        bool isEnabled,
        DateTime occurredOn)
    {
        PartnerAccountId = partnerAccountId;
        AccountNumber = accountNumber;
        RIB = rib;
        Domiciliation = domiciliation;
        BusinessName = businessName;
        ShortName = shortName;
        AccountBalance = accountBalance;
        BankId = bankId;
        AccountTypeId = accountTypeId;
        IsEnabled = isEnabled;
        OccurredOn = occurredOn;
    }
}