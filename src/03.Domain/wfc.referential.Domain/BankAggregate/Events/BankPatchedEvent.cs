using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.BankAggregate.Events;

public record BankPatchedEvent : IDomainEvent
{
    public Guid BankId { get; }
    public string Code { get; }
    public string Name { get; }
    public string Abbreviation { get; }
    public bool IsEnabled { get; }
    public DateTime OccurredOn { get; }

    public BankPatchedEvent(
        Guid bankId,
        string code,
        string name,
        string abbreviation,
        bool isEnabled,
        DateTime occurredOn)
    {
        BankId = bankId;
        Code = code;
        Name = name;
        Abbreviation = abbreviation;
        IsEnabled = isEnabled;
        OccurredOn = occurredOn;
    }
}