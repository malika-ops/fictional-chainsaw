using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Domain.ProductAggregate;
using wfc.referential.Domain.ServiceAggregate.Events;
using wfc.referential.Domain.TaxRuleDetailAggregate;

namespace wfc.referential.Domain.ServiceAggregate;

public class Service : Aggregate<ServiceId>
{
    public string Code { get; private set; }
    public string Name { get; private set; }
    public FlowDirection FlowDirection { get; private set; } = FlowDirection.None;
    public bool IsEnabled { get; private set; } = true;

    public ProductId ProductId { get; private set; }

    public ICollection<TaxRuleDetail> TaxRuleDetails { get; private set; }
    private Service() { }

    public static Service Create(ServiceId id, string code, string name, FlowDirection flowDirection, bool isEnabled, ProductId productId)
    {
        var service = new Service
        {
            Id = id,
            Code = code,
            Name = name,
            FlowDirection = flowDirection,
            IsEnabled = isEnabled,
            ProductId = productId
        };

        service.AddDomainEvent(new ServiceCreatedEvent(id.Value, code, name,flowDirection, isEnabled, productId.Value));
        return service;
    }

    public void SetInactive()
    {
        IsEnabled = false;
        AddDomainEvent(new ServiceStatusChangedEvent(Id.Value, IsEnabled, DateTime.UtcNow));
    }

    public void Update(string code, string name, FlowDirection flowDirection, bool isEnabled, ProductId productId)
    {
        Code = code;
        Name = name;
        FlowDirection = flowDirection;
        IsEnabled = isEnabled;
        ProductId = productId;

        AddDomainEvent(new ServiceUpdatedEvent(Id.Value, Code, Name, FlowDirection, IsEnabled, DateTime.UtcNow, ProductId.Value));
    }

    public void Patch(string? code, string? name, FlowDirection? flowDirection, bool? isEnabled, ProductId? productId)
    {
        Code = code ?? Code;
        Name = name ?? Name;
        FlowDirection = flowDirection ?? FlowDirection;
        IsEnabled = isEnabled ?? IsEnabled;
        ProductId = productId ?? ProductId;

        AddDomainEvent(new ServicePatchedEvent(Id.Value, Code, Name, FlowDirection, IsEnabled, DateTime.UtcNow, ProductId.Value));
    }
}

