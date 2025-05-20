using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.ProductAggregate.Events;

public class ProductPatchedEvent : IDomainEvent
{
    public Guid ProductId { get; }
    public string Code { get; }
    public string Name { get; }
    public bool IsEnabled { get; }
    public DateTime OccurredOn { get; }

    public ProductPatchedEvent(
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
