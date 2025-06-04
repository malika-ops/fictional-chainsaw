using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Domain.ProductAggregate.Events;
using wfc.referential.Domain.ServiceAggregate;

namespace wfc.referential.Domain.ProductAggregate;

public class Product : Aggregate<ProductId>
{
    public string Code { get; private set; }
    public string Name { get; private set; }
    public bool IsEnabled { get; private set; } = true;

    public List<Service> Services { get; private set; } = [];

    private Product() { }

    public static Product Create(ProductId id, string code, string name, bool isEnabled)
    {
        var product = new Product
        {
            Id = id,
            Code = code,
            Name = name,
            IsEnabled = isEnabled
        };

        // raise the creation event
        product.AddDomainEvent(new ProductCreatedEvent(
            product.Id.Value,
            product.Code,
            product.Name,
            product.IsEnabled
            ));
        return product;
    }

    public void SetInactive()
    {
        IsEnabled = false;

        // Raise the status changed event
        AddDomainEvent(new ProductStatusChangedEvent(
            Id.Value,
            IsEnabled,
            DateTime.UtcNow
        ));
    }
    public void Update(string code, string name, bool isEnabled)
    {
        Code = code;
        Name = name;
        IsEnabled = isEnabled;

        // raise the update event
        AddDomainEvent(new ProductUpdatedEvent(
            Id.Value,
            Code,
            Name,
            IsEnabled,
            DateTime.UtcNow
        ));
    }
    public void Patch(string? code, string? name, bool? isEnabled)
    {
        Code = code ?? Code;
        Name = name ?? Name;
        IsEnabled = isEnabled  ?? IsEnabled;

        AddDomainEvent(new ProductPatchedEvent(
            Id.Value,
            Code,
            Name,
            IsEnabled,
            DateTime.UtcNow
        ));
    }

}
