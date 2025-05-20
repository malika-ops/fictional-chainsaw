using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.TaxAggregate.Events;

public record TaxStatusChangedEvent : IDomainEvent
{
    public Guid TaxId { get; }
    public bool IsEnabled { get; }
    public DateTime OccurredOn { get; init; } = DateTime.Now;

    public TaxStatusChangedEvent(Tax tax)
    {
        TaxId = tax.Id!.Value;
        IsEnabled = tax.IsEnabled;
    }
}
