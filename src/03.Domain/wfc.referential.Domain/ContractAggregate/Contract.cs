using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Domain.ContractAggregate.Events;
using wfc.referential.Domain.PartnerAggregate;

namespace wfc.referential.Domain.ContractAggregate;

public class Contract : Aggregate<ContractId>
{
    public string Code { get; private set; } = string.Empty;
    public PartnerId PartnerId { get; private set; } = default!;
    public Partner? Partner { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public bool IsEnabled { get; private set; } = true;

    private Contract() { }

    public static Contract Create(
        ContractId id,
        string code,
        PartnerId partnerId,
        DateTime startDate,
        DateTime endDate)
    {
        var contract = new Contract
        {
            Id = id,
            Code = code,
            PartnerId = partnerId,
            StartDate = startDate,
            EndDate = endDate,
            IsEnabled = true
        };

        contract.AddDomainEvent(new ContractCreatedEvent(
            contract.Id.Value,
            contract.Code,
            contract.PartnerId.Value,
            contract.StartDate,
            contract.EndDate,
            contract.IsEnabled,
            DateTime.UtcNow));

        return contract;
    }

    public void Update(
        string code,
        PartnerId partnerId,
        DateTime startDate,
        DateTime endDate,
        bool? isEnabled)
    {
        Code = code;
        PartnerId = partnerId;
        StartDate = startDate;
        EndDate = endDate;
        IsEnabled = isEnabled ?? IsEnabled;

        AddDomainEvent(new ContractUpdatedEvent(
            Id.Value,
            Code,
            PartnerId.Value,
            StartDate,
            EndDate,
            IsEnabled,
            DateTime.UtcNow));
    }

    public void Patch(
        string? code,
        PartnerId? partnerId,
        DateTime? startDate,
        DateTime? endDate,
        bool? isEnabled)
    {
        Code = code ?? Code;
        PartnerId = partnerId ?? PartnerId;
        StartDate = startDate ?? StartDate;
        EndDate = endDate ?? EndDate;
        IsEnabled = isEnabled ?? IsEnabled;

        AddDomainEvent(new ContractPatchedEvent(
            Id.Value,
            Code,
            PartnerId.Value,
            StartDate,
            EndDate,
            IsEnabled,
            DateTime.UtcNow));
    }

    public void Disable()
    {
        IsEnabled = false;

        AddDomainEvent(new ContractDisabledEvent(
            Id.Value,
            DateTime.UtcNow));
    }

    public void Activate()
    {
        IsEnabled = true;

        AddDomainEvent(new ContractActivatedEvent(
            Id.Value,
            DateTime.UtcNow));
    }

    public void SetPartner(PartnerId partnerId, Partner? partner = null)
    {
        PartnerId = partnerId;
        if (partner != null)
            Partner = partner;
    }
}