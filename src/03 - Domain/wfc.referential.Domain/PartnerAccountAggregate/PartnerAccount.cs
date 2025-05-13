using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Domain.BankAggregate;
using wfc.referential.Domain.PartnerAccountAggregate.Events;

namespace wfc.referential.Domain.PartnerAccountAggregate;

public class PartnerAccount : Aggregate<PartnerAccountId>
{
    public string AccountNumber { get; private set; } = string.Empty;
    public string RIB { get; private set; } = string.Empty;
    public string Domiciliation { get; private set; } = string.Empty;
    public string BusinessName { get; private set; } = string.Empty;
    public string ShortName { get; private set; } = string.Empty;
    public decimal AccountBalance { get; private set; } = 0;
    public Bank Bank { get; private set; }
    public BankId BankId { get; private set; }
    public AccountType AccountType { get; private set; }
    public bool IsEnabled { get; private set; } = true;

    private PartnerAccount() { }

    public static PartnerAccount Create(
        PartnerAccountId id,
        string accountNumber,
        string rib,
        string domiciliation,
        string businessName,
        string shortName,
        decimal accountBalance,
        Bank bank,
        AccountType accountType)
    {
        var partnerAccount = new PartnerAccount
        {
            Id = id,
            AccountNumber = accountNumber,
            RIB = rib,
            Domiciliation = domiciliation,
            BusinessName = businessName,
            ShortName = shortName,
            AccountBalance = accountBalance,
            Bank = bank,
            BankId = bank.Id,
            AccountType = accountType,
            IsEnabled = true
        };

        // raise the creation event
        partnerAccount.AddDomainEvent(new PartnerAccountCreatedEvent(
            partnerAccount.Id.Value,
            partnerAccount.AccountNumber,
            partnerAccount.RIB,
            partnerAccount.Domiciliation,
            partnerAccount.BusinessName,
            partnerAccount.ShortName,
            partnerAccount.AccountBalance,
            partnerAccount.Bank.Id.Value,
            partnerAccount.AccountType,
            partnerAccount.IsEnabled,
            DateTime.UtcNow
        ));
        return partnerAccount;
    }

    public void Update(
        string accountNumber,
        string rib,
        string domiciliation,
        string businessName,
        string shortName,
        decimal accountBalance,
        Bank bank,
        AccountType accountType)
    {
        AccountNumber = accountNumber;
        RIB = rib;
        Domiciliation = domiciliation;
        BusinessName = businessName;
        ShortName = shortName;
        AccountBalance = accountBalance;
        Bank = bank;
        BankId = bank.Id;
        AccountType = accountType;

        // raise the update event
        AddDomainEvent(new PartnerAccountUpdatedEvent(
            Id.Value,
            AccountNumber,
            RIB,
            Domiciliation,
            BusinessName,
            ShortName,
            AccountBalance,
            Bank.Id.Value,
            AccountType,
            IsEnabled,
            DateTime.UtcNow
        ));
    }

    public void Patch(
        string accountNumber,
        string rib,
        string domiciliation,
        string businessName,
        string shortName,
        decimal accountBalance,
        Bank bank,
        AccountType accountType)
    {
        AccountNumber = accountNumber;
        RIB = rib;
        Domiciliation = domiciliation;
        BusinessName = businessName;
        ShortName = shortName;
        AccountBalance = accountBalance;
        Bank = bank;
        BankId = bank.Id;
        AccountType = accountType;

        AddDomainEvent(new PartnerAccountPatchedEvent(
            Id.Value,
            AccountNumber,
            RIB,
            Domiciliation,
            BusinessName,
            ShortName,
            AccountBalance,
            Bank.Id.Value,
            AccountType,
            IsEnabled,
            DateTime.UtcNow
        ));
    }

    public void UpdateBalance(decimal newBalance)
    {
        var oldBalance = AccountBalance;
        AccountBalance = newBalance;

        AddDomainEvent(new PartnerAccountBalanceUpdatedEvent(
            Id.Value,
            oldBalance,
            AccountBalance,
            DateTime.UtcNow
        ));
    }

    public void Disable()
    {
        IsEnabled = false;

        // raise the disable event
        AddDomainEvent(new PartnerAccountDisabledEvent(
            Id.Value,
            DateTime.UtcNow
        ));
    }

    public void Activate()
    {
        IsEnabled = true;

        // raise the activate event
        AddDomainEvent(new PartnerAccountActivatedEvent(
            Id.Value,
            DateTime.UtcNow
        ));
    }
}