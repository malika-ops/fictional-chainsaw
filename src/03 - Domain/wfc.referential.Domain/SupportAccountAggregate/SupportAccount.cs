using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Domain.SupportAccountAggregate.Events;
using wfc.referential.Domain.PartnerAggregate;

namespace wfc.referential.Domain.SupportAccountAggregate;

public class SupportAccount : Aggregate<SupportAccountId>
{
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public decimal Threshold { get; private set; } = 0;
    public decimal Limit { get; private set; } = 0;
    public decimal AccountBalance { get; private set; } = 0;
    public string AccountingNumber { get; private set; } = string.Empty;
    public bool IsEnabled { get; private set; } = true;
    public Partner Partner { get; private set; }
    public PartnerId PartnerId { get; private set; }
    public SupportAccountType SupportAccountType { get; private set; }

    private SupportAccount() { }

    public static SupportAccount Create(
        SupportAccountId id,
        string code,
        string name,
        decimal threshold,
        decimal limit,
        decimal accountBalance,
        string accountingNumber,
        Partner partner,
        SupportAccountType supportAccountType)
    {
        var supportAccount = new SupportAccount
        {
            Id = id,
            Code = code,
            Name = name,
            Threshold = threshold,
            Limit = limit,
            AccountBalance = accountBalance,
            AccountingNumber = accountingNumber,
            Partner = partner,
            PartnerId = partner.Id,
            SupportAccountType = supportAccountType,
            IsEnabled = true
        };

        supportAccount.AddDomainEvent(new SupportAccountCreatedEvent(
            supportAccount.Id.Value,
            supportAccount.Code,
            supportAccount.Name,
            supportAccount.Threshold,
            supportAccount.Limit,
            supportAccount.AccountBalance,
            supportAccount.AccountingNumber,
            supportAccount.PartnerId.Value,
            supportAccount.SupportAccountType,
            supportAccount.IsEnabled,
            DateTime.UtcNow
        ));
        return supportAccount;
    }

    public void Update(
        string code,
        string name,
        decimal threshold,
        decimal limit,
        decimal accountBalance,
        string accountingNumber,
        Partner partner,
        SupportAccountType supportAccountType)
    {
        Code = code;
        Name = name;
        Threshold = threshold;
        Limit = limit;
        AccountBalance = accountBalance;
        AccountingNumber = accountingNumber;
        Partner = partner;
        PartnerId = partner.Id;
        SupportAccountType = supportAccountType;

        AddDomainEvent(new SupportAccountUpdatedEvent(
            Id.Value,
            Code,
            Name,
            Threshold,
            Limit,
            AccountBalance,
            AccountingNumber,
            PartnerId.Value,
            SupportAccountType,
            IsEnabled,
            DateTime.UtcNow
        ));
    }

    public void Patch(
        string code,
        string name,
        decimal threshold,
        decimal limit,
        decimal accountBalance,
        string accountingNumber,
        Partner partner,
        SupportAccountType supportAccountType)
    {
        Code = code;
        Name = name;
        Threshold = threshold;
        Limit = limit;
        AccountBalance = accountBalance;
        AccountingNumber = accountingNumber;
        Partner = partner;
        PartnerId = partner.Id;
        SupportAccountType = supportAccountType;

        AddDomainEvent(new SupportAccountPatchedEvent(
            Id.Value,
            Code,
            Name,
            Threshold,
            Limit,
            AccountBalance,
            AccountingNumber,
            PartnerId.Value,
            SupportAccountType,
            IsEnabled,
            DateTime.UtcNow
        ));
    }

    public void UpdateBalance(decimal newBalance)
    {
        var oldBalance = AccountBalance;
        AccountBalance = newBalance;

        AddDomainEvent(new SupportAccountBalanceUpdatedEvent(
            Id.Value,
            oldBalance,
            AccountBalance,
            DateTime.UtcNow
        ));
    }

    public void Disable()
    {
        IsEnabled = false;

        AddDomainEvent(new SupportAccountDisabledEvent(
            Id.Value,
            DateTime.UtcNow
        ));
    }

    public void Activate()
    {
        IsEnabled = true;

        AddDomainEvent(new SupportAccountActivatedEvent(
            Id.Value,
            DateTime.UtcNow
        ));
    }
}