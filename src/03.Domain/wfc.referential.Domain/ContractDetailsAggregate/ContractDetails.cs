using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Domain.ContractDetailsAggregate.Events;
using wfc.referential.Domain.ContractAggregate;
using wfc.referential.Domain.PricingAggregate;

namespace wfc.referential.Domain.ContractDetailsAggregate;

public class ContractDetails : Aggregate<ContractDetailsId>
{
    public ContractId ContractId { get; private set; } = default!;
    public Contract? Contract { get; private set; }
    public PricingId PricingId { get; private set; } = default!;
    public Pricing? Pricing { get; private set; }
    public bool IsEnabled { get; private set; } = true;

    private ContractDetails() { }

    public static ContractDetails Create(
        ContractDetailsId id,
        ContractId contractId,
        PricingId pricingId)
    {
        var contractDetails = new ContractDetails
        {
            Id = id,
            ContractId = contractId,
            PricingId = pricingId,
            IsEnabled = true
        };

        contractDetails.AddDomainEvent(new ContractDetailsCreatedEvent(
            contractDetails.Id.Value,
            contractDetails.ContractId.Value,
            contractDetails.PricingId.Value,
            contractDetails.IsEnabled,
            DateTime.UtcNow));

        return contractDetails;
    }

    public void Update(
        ContractId contractId,
        PricingId pricingId,
        bool? isEnabled)
    {
        ContractId = contractId;
        PricingId = pricingId;
        IsEnabled = isEnabled ?? IsEnabled;

        AddDomainEvent(new ContractDetailsUpdatedEvent(
            Id.Value,
            ContractId.Value,
            PricingId.Value,
            IsEnabled,
            DateTime.UtcNow));
    }

    public void Patch(
        ContractId? contractId,
        PricingId? pricingId,
        bool? isEnabled)
    {
        ContractId = contractId ?? ContractId;
        PricingId = pricingId ?? PricingId;
        IsEnabled = isEnabled ?? IsEnabled;

        AddDomainEvent(new ContractDetailsPatchedEvent(
            Id.Value,
            ContractId.Value,
            PricingId.Value,
            IsEnabled,
            DateTime.UtcNow));
    }

    public void Disable()
    {
        IsEnabled = false;

        AddDomainEvent(new ContractDetailsDisabledEvent(
            Id.Value,
            DateTime.UtcNow));
    }

    public void Activate()
    {
        IsEnabled = true;

        AddDomainEvent(new ContractDetailsActivatedEvent(
            Id.Value,
            DateTime.UtcNow));
    }

    public void SetContract(ContractId contractId, Contract? contract = null)
    {
        ContractId = contractId;
        if (contract != null)
            Contract = contract;
    }

    public void SetPricing(PricingId pricingId, Pricing? pricing = null)
    {
        PricingId = pricingId;
        if (pricing != null)
            Pricing = pricing;
    }
}