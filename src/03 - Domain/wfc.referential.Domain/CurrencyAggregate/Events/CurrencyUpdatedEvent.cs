using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.CurrencyAggregate.Events;

public record CurrencyUpdatedEvent : IDomainEvent
{
    public Guid CurrencyId { get; }
    public string Code { get; } = string.Empty;
    public string CodeAR { get; } = string.Empty;
    public string CodeEN { get; } = string.Empty;
    public string Name { get; } = string.Empty;
    public int CodeIso { get; }
    public bool IsEnabled { get; }
    public DateTime OccurredOn { get; }

    public CurrencyUpdatedEvent(
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