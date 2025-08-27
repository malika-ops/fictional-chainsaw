using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Domain.BankAggregate;
using wfc.referential.Domain.PartnerAccountAggregate.Events;

namespace wfc.referential.Domain.PartnerAccountAggregate;

public class PartnerAccount : Aggregate<PartnerAccountId>
{
    public string AccountNumber { get; private set; } = string.Empty;
    public string RIB { get; private set; } = string.Empty;
    public string? Domiciliation { get; private set; } = string.Empty;
    public string? BusinessName { get; private set; } = string.Empty;
    public string? ShortName { get; private set; } = string.Empty;
    public decimal AccountBalance { get; private set; } = 0;
    public Bank Bank { get; private set; }
    public BankId BankId { get; private set; }
    public PartnerAccountTypeEnum PartnerAccountType { get; private set; }
    public bool IsEnabled { get; private set; } = true;

    private PartnerAccount() { }

    public static PartnerAccount Create(
        PartnerAccountId id,
        string accountNumber,
        string rib,
        string? domiciliation,
        string? businessName,
        string? shortName,
        decimal accountBalance,
        Bank bank,
        PartnerAccountTypeEnum partnerAccountType)
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
            PartnerAccountType = partnerAccountType,
            IsEnabled = true
        };

        // raise the creation event
        partnerAccount.AddDomainEvent(new PartnerAccountCreatedEvent(
            partnerAccount.Id.Value,
            partnerAccount.AccountNumber,
            partnerAccount.RIB,
            partnerAccount.Domiciliation ?? string.Empty,
            partnerAccount.BusinessName ?? string.Empty,
            partnerAccount.ShortName ?? string.Empty,
            partnerAccount.AccountBalance,
            partnerAccount.Bank.Id.Value,
            partnerAccount.IsEnabled,
            DateTime.UtcNow
        ));
        return partnerAccount;
    }

    public void Update(
        string accountNumber,
        string rib,
        string? domiciliation,
        string? businessName,
        string? shortName,
        decimal accountBalance,
        Bank bank,
        PartnerAccountTypeEnum partnerAccountType)
    {
        AccountNumber = accountNumber;
        RIB = rib;
        Domiciliation = domiciliation;
        BusinessName = businessName;
        ShortName = shortName;
        AccountBalance = accountBalance;
        Bank = bank;
        BankId = bank.Id;
        PartnerAccountType = partnerAccountType;

        // raise the update event
        AddDomainEvent(new PartnerAccountUpdatedEvent(
            Id.Value,
            AccountNumber,
            RIB,
            Domiciliation ?? string.Empty,
            BusinessName ?? string.Empty,
            ShortName ?? string.Empty,
            AccountBalance,
            Bank.Id.Value,
            IsEnabled,
            DateTime.UtcNow
        ));
    }

    public void Patch(
        string? accountNumber,
        string? rib,
        string? domiciliation,
        string? businessName,
        string? shortName,
        decimal? accountBalance,
        Bank? bank,
        PartnerAccountTypeEnum? partnerAccountType,
        bool? isEnabled)
    {
        AccountNumber = accountNumber ?? AccountNumber;
        RIB = rib ?? RIB;
        Domiciliation = domiciliation ?? Domiciliation;
        BusinessName = businessName ?? BusinessName;
        ShortName = shortName ?? ShortName;
        AccountBalance = accountBalance ?? AccountBalance;
        PartnerAccountType = partnerAccountType ?? PartnerAccountType;

        if (bank is not null)
        {
            Bank = bank;
            BankId = bank.Id;
        }

        AddDomainEvent(new PartnerAccountPatchedEvent(
            Id.Value,
            AccountNumber,
            RIB,
            Domiciliation ?? string.Empty,
            BusinessName ?? string.Empty,
            ShortName ?? string.Empty,
            AccountBalance,
            Bank.Id.Value,
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