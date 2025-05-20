using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.ProductAggregate.Events;

public class ProductCreatedEvent : IDomainEvent
{
    public Guid ProductId { get; }
    public string Code { get; }
    public string Name { get; }
    public bool IsEnabled { get; }

    public ProductCreatedEvent(
        Guid productId,
        string code,
        string name,
        bool isEnabled
        )
    {
        ProductId = productId;
        Code = code;
        Name = name;
        IsEnabled = isEnabled;
    }
}