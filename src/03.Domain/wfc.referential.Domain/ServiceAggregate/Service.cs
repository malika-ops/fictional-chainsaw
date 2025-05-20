using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Domain.ProductAggregate;
using wfc.referential.Domain.ServiceAggregate.Events;
using wfc.referential.Domain.TaxRuleDetailAggregate;

namespace wfc.referential.Domain.ServiceAggregate;

public class Service : Aggregate<ServiceId>
{
    public string Code { get; private set; }
    public string Name { get; private set; }
    public bool IsEnabled { get; private set; } = true;

    public ProductId ProductId { get; private set; }

    public ICollection<TaxRuleDetailAggregate.TaxRuleDetail> TaxRuleDetails { get; private set; }
    private Service() { }

    public static Service Create(ServiceId id, string code, string name, bool isEnabled, ProductId productId)
    {
        var service = new Service
        {
            Id = id,
            Code = code,
            Name = name,
            IsEnabled = isEnabled,
            ProductId = productId
        };

        service.AddDomainEvent(new ServiceCreatedEvent(id.Value, code, name, isEnabled, productId.Value));
        return service;
    }

    public void SetInactive()
    {
        IsEnabled = false;
        AddDomainEvent(new ServiceStatusChangedEvent(Id.Value, IsEnabled, DateTime.UtcNow));
    }

    public void Update(string code, string name, bool isEnabled, ProductId productId)
    {
        Code = code;
        Name = name;
        IsEnabled = isEnabled;
        ProductId = productId;

        AddDomainEvent(new ServiceUpdatedEvent(Id.Value, Code, Name, IsEnabled, DateTime.UtcNow, ProductId.Value));
    }

    public void Patch()
    {
        AddDomainEvent(new ServicePatchedEvent(Id.Value, Code, Name, IsEnabled, DateTime.UtcNow, ProductId.Value));
    }
}

