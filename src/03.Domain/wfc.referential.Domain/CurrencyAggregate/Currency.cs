using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.CurrencyAggregate.Events;

namespace wfc.referential.Domain.CurrencyAggregate;

public class Currency : Aggregate<CurrencyId>
{
    public string Code { get; private set; } = string.Empty;
    public string CodeAR { get; private set; } = string.Empty;
    public string CodeEN { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public int CodeIso { get; private set; }
    public bool IsEnabled { get; private set; } = true;
    public List<Country> Countries { get; private set; } = [];

    private Currency() { }

    public static Currency Create(
        CurrencyId id,
        string code,
        string codeAR,
        string codeEN,
        string name,
        int codeIso)
    {
        var currency = new Currency
        {
            Id = id,
            Code = code,
            CodeAR = codeAR,
            CodeEN = codeEN,
            Name = name,
            CodeIso = codeIso,
            IsEnabled = true,
            Countries = []
        };

        currency.AddDomainEvent(new CurrencyCreatedEvent(
            currency.Id.Value,
            currency.Code,
            currency.CodeAR,
            currency.CodeEN,
            currency.Name,
            currency.CodeIso,
            DateTime.UtcNow));

        return currency;
    }

    public void Update(
        string code,
        string codeAR,
        string codeEN,
        string name,
        int codeIso,
        bool? isEnabled)
    {
        Code = code;
        CodeAR = codeAR;
        CodeEN = codeEN;
        Name = name;
        CodeIso = codeIso;
        IsEnabled = isEnabled ?? IsEnabled;

        AddDomainEvent(new CurrencyUpdatedEvent(
            Id.Value,
            Code,
            CodeAR,
            CodeEN,
            Name,
            CodeIso,
            DateTime.UtcNow));
    }

    public void Patch(
        string? code,
        string? codeAR,
        string? codeEN,
        string? name,
        int? codeIso,
        bool? isEnabled)
    {
        Code = code ?? Code;
        CodeAR = codeAR ?? CodeAR;
        CodeEN = codeEN ?? CodeEN;
        Name = name ?? Name;
        CodeIso = codeIso ?? CodeIso;
        IsEnabled = isEnabled ?? IsEnabled;

        AddDomainEvent(new CurrencyPatchedEvent(
            Id.Value,
            Code,
            CodeAR,
            CodeEN,
            Name,
            CodeIso,
            DateTime.UtcNow));
    }

    public void Disable()
    {
        IsEnabled = false;

        AddDomainEvent(new CurrencyDisabledEvent(
            Id.Value,
            DateTime.UtcNow));
    }

    public void Activate()
    {
        IsEnabled = true;

        AddDomainEvent(new CurrencyActivatedEvent(
            Id.Value,
            DateTime.UtcNow));
    }
}