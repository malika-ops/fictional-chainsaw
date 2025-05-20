using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.CurrencyAggregate.Events;

public record CurrencyCreatedEvent : IDomainEvent
{
    public Guid CurrencyId { get; }
    public string Code { get; }
    public string CodeAR { get; }
    public string CodeEN { get; }
    public string Name { get; }
    public int CodeIso { get; }
    public bool IsEnabled { get; }
    public DateTime OccurredOn { get; }

    public CurrencyCreatedEvent(
        Guid currencyId,
        string code,
        string codeAR,
        string codeEN,
        string name,
        int codeiso,
        bool isEnabled,
        DateTime occurredOn)
    {
        CurrencyId = currencyId;
        Code = code;
        CodeAR = codeAR;
        CodeEN = codeEN;
        Name = name;
        CodeIso = codeiso;
        IsEnabled = isEnabled;
        OccurredOn = occurredOn;
    }
}

