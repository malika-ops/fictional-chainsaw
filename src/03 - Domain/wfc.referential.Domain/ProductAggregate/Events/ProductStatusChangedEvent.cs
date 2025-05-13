using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.ProductAggregate.Events;

public class ProductStatusChangedEvent : IDomainEvent
{
    public Guid ProductId { get; }
    public bool IsEnabled { get; }
    public DateTime OccurredOn { get; }

    public ProductStatusChangedEvent(
        Guid productId,
        bool isEnabled,
        DateTime occurredOn)
    {
        ProductId = productId;
        IsEnabled = isEnabled;
        OccurredOn = occurredOn;
    }
}
