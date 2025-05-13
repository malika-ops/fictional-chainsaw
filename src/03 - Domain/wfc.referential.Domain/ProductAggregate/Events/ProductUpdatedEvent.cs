using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.ProductAggregate.Events;

public class ProductUpdatedEvent : IDomainEvent
{
    public Guid ProductId { get; }
    public string Code { get; } = string.Empty;
    public string Name { get; } = string.Empty;
    public bool IsEnabled { get; }
    public DateTime OccurredOn { get; }

    public ProductUpdatedEvent(
        Guid productId,
        string code,
        string name,
        bool isEnabled,
        DateTime occurredOn)
    {
        ProductId = productId;
        Code = code;
        Name = name;
        IsEnabled = isEnabled;
        OccurredOn = occurredOn;
    }
}
