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

    public static Currency Create(CurrencyId id, string code, string codeAR, string codeEN, string name, int codeiso)
    {
        var currency = new Currency
        {
            Id = id,
            Code = code,
            CodeAR = codeAR,
            CodeEN = codeEN,
            Name = name,
            CodeIso = codeiso,
            IsEnabled = true,
            Countries = []
        };

        // Raise the creation event
        currency.AddDomainEvent(new CurrencyCreatedEvent(
            currency.Id.Value,
            currency.Code,
            currency.CodeAR,
            currency.CodeEN,
            currency.Name,
            currency.CodeIso,
            currency.IsEnabled,
            DateTime.UtcNow
        ));

        return currency;
    }

    public void Update(
            string code,
            string codeAR,
            string codeEN,
            string name,
            int codeiso)
    {
        Code = code;
        CodeAR = codeAR;
        CodeEN = codeEN;
        Name = name;
        CodeIso = codeiso;
        // Raise the update event
        AddDomainEvent(new CurrencyUpdatedEvent(
            Id.Value,
            Code,
            CodeAR,
            CodeEN,
            Name,
            CodeIso,
            IsEnabled,
            DateTime.UtcNow
        ));
    }

    public void Patch(
            string code,
            string codeAR,
            string codeEN,
            string name,
            int codeiso)
    {
        Code = code;
        CodeAR = codeAR;
        CodeEN = codeEN;
        Name = name;
        CodeIso = codeiso;

        // Raise the patch event
        AddDomainEvent(new CurrencyPatchedEvent(
            Id.Value,
            Code,
            CodeAR,
            CodeEN,
            Name,
            CodeIso,
            IsEnabled,
            DateTime.UtcNow
        ));
    }

    public void Disable()
    {
        IsEnabled = false;

        // Raise the disable event
        AddDomainEvent(new CurrencyDisabledEvent(
            Id.Value,
            DateTime.UtcNow
        ));
    }

    public void Activate()
    {
        IsEnabled = true;

        // Raise the activate event
        AddDomainEvent(new CurrencyActivatedEvent(
            Id.Value,
            DateTime.UtcNow
        ));
    }
}