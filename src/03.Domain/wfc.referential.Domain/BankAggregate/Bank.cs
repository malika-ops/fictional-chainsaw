using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Domain.BankAggregate.Events;

namespace wfc.referential.Domain.BankAggregate;

public class Bank : Aggregate<BankId>
{
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Abbreviation { get; private set; } = string.Empty;
    public bool IsEnabled { get; private set; } = true;

    private Bank() { }

    public static Bank Create(BankId id, string code, string name, string abbreviation)
    {
        var bank = new Bank
        {
            Id = id,
            Code = code,
            Name = name,
            Abbreviation = abbreviation,
            IsEnabled = true
        };

        bank.AddDomainEvent(new BankCreatedEvent(
            bank.Id.Value,
            bank.Code,
            bank.Name,
            bank.Abbreviation,
            bank.IsEnabled,
            DateTime.UtcNow));

        return bank;
    }

    public void Update(
        string code,
        string name,
        string abbreviation,
        bool? isEnabled)
    {
        Code = code;
        Name = name;
        Abbreviation = abbreviation;
        IsEnabled = isEnabled ?? IsEnabled;

        AddDomainEvent(new BankUpdatedEvent(
            Id.Value,
            Code,
            Name,
            Abbreviation,
            IsEnabled,
            DateTime.UtcNow));
    }

    public void Patch(
        string? code,
        string? name,
        string? abbreviation,
        bool? isEnabled)
    {
        Code = code ?? Code;
        Name = name ?? Name;
        Abbreviation = abbreviation ?? Abbreviation;
        IsEnabled = isEnabled ?? IsEnabled;

        AddDomainEvent(new BankPatchedEvent(
            Id.Value,
            Code,
            Name,
            Abbreviation,
            IsEnabled,
            DateTime.UtcNow));
    }

    public void Disable()
    {
        IsEnabled = false;

        AddDomainEvent(new BankDisabledEvent(
            Id.Value,
            DateTime.UtcNow));
    }

    public void Activate()
    {
        IsEnabled = true;

        AddDomainEvent(new BankActivatedEvent(
            Id.Value,
            DateTime.UtcNow));
    }
}