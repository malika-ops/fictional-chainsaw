using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Domain.PartnerAggregate;
using wfc.referential.Domain.SupportAccountAggregate.Events;

namespace wfc.referential.Domain.SupportAccountAggregate;

public class SupportAccount : Aggregate<SupportAccountId>
{
    public string Code { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal Threshold { get; private set; } = 0;
    public decimal Limit { get; private set; } = 0;
    public decimal AccountBalance { get; private set; } = 0;
    public string AccountingNumber { get; private set; } = string.Empty;
    public bool IsEnabled { get; private set; } = true;
    public Partner? Partner { get; private set; }
    public PartnerId? PartnerId { get; private set; }
    public SupportAccountTypeEnum SupportAccountType { get; private set; }

    private SupportAccount() { }

    public static SupportAccount Create(
        SupportAccountId id,
        string code,
        string description,
        decimal threshold,
        decimal limit,
        decimal accountBalance,
        string accountingNumber,
        SupportAccountTypeEnum supportAccountType)
    {
        var supportAccount = new SupportAccount
        {
            Id = id,
            Code = code,
            Description = description,
            Threshold = threshold,
            Limit = limit,
            AccountBalance = accountBalance,
            AccountingNumber = accountingNumber,
            SupportAccountType = supportAccountType,
            IsEnabled = true
        };

        supportAccount.AddDomainEvent(new SupportAccountCreatedEvent(
            supportAccount.Id.Value,
            supportAccount.Code,
            supportAccount.Description,
            supportAccount.Threshold,
            supportAccount.Limit,
            supportAccount.AccountBalance,
            supportAccount.AccountingNumber,
            supportAccount.IsEnabled,
            DateTime.UtcNow));

        return supportAccount;
    }

    public void Update(
        string code,
        string description,
        decimal threshold,
        decimal limit,
        decimal accountBalance,
        string accountingNumber,
        SupportAccountTypeEnum supportAccountType,
        bool? isEnabled)
    {
        Code = code;
        Description = description;
        Threshold = threshold;
        Limit = limit;
        AccountBalance = accountBalance;
        AccountingNumber = accountingNumber;
        SupportAccountType = supportAccountType;
        IsEnabled = isEnabled ?? IsEnabled;

        AddDomainEvent(new SupportAccountUpdatedEvent(
            Id.Value,
            Code,
            Description,
            Threshold,
            Limit,
            AccountBalance,
            AccountingNumber,
            IsEnabled,
            DateTime.UtcNow));
    }

    public void Patch(
        string? code,
        string? description,
        decimal? threshold,
        decimal? limit,
        decimal? accountBalance,
        string? accountingNumber,
        SupportAccountTypeEnum? supportAccountType,
        bool? isEnabled)
    {
        Code = code ?? Code;
        Description = description ?? Description;
        Threshold = threshold ?? Threshold;
        Limit = limit ?? Limit;
        AccountBalance = accountBalance ?? AccountBalance;
        AccountingNumber = accountingNumber ?? AccountingNumber;
        SupportAccountType = supportAccountType ?? SupportAccountType;
        IsEnabled = isEnabled ?? IsEnabled;

        AddDomainEvent(new SupportAccountPatchedEvent(
            Id.Value,
            Code,
            Description,
            Threshold,
            Limit,
            AccountBalance,
            AccountingNumber,
            IsEnabled,
            DateTime.UtcNow));
    }

    public void UpdateBalance(decimal newBalance)
    {
        var oldBalance = AccountBalance;
        AccountBalance = newBalance;

        AddDomainEvent(new SupportAccountBalanceUpdatedEvent(
            Id.Value,
            oldBalance,
            AccountBalance,
            DateTime.UtcNow));
    }

    public void Disable()
    {
        IsEnabled = false;

        AddDomainEvent(new SupportAccountDisabledEvent(
            Id.Value,
            DateTime.UtcNow));
    }

    public void Activate()
    {
        IsEnabled = true;

        AddDomainEvent(new SupportAccountActivatedEvent(
            Id.Value,
            DateTime.UtcNow));
    }
}