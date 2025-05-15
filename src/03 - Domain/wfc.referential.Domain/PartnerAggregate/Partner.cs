using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Domain.PartnerAggregate.Events;
using wfc.referential.Domain.SupportAccountAggregate;
using wfc.referential.Domain.PartnerAccountAggregate;

namespace wfc.referential.Domain.PartnerAggregate;

public class Partner : Aggregate<PartnerId>
{
    public string Code { get; private set; } = string.Empty;
    public string Label { get; private set; } = string.Empty;
    public NetworkMode NetworkMode { get; private set; }
    public PaymentMode PaymentMode { get; private set; }
    public string Type { get; private set; } = string.Empty;
    public Guid? IdParent { get; private set; }
    public SupportAccountType SupportAccountType { get; private set; }
    public string TaxIdentificationNumber { get; private set; } = string.Empty;
    public string TaxRegime { get; private set; } = string.Empty;
    public string AuxiliaryAccount { get; private set; } = string.Empty;
    public string ICE { get; private set; } = string.Empty;
    public string RASRate { get; private set; } = string.Empty;
    public string Logo { get; private set; } = string.Empty;
    public bool IsEnabled { get; private set; } = true;

    // Account relationships
    public Guid? CommissionAccountId { get; private set; }
    public PartnerAccount CommissionAccount { get; private set; }

    public Guid? ActivityAccountId { get; private set; }
    public PartnerAccount ActivityAccount { get; private set; }

    public Guid? SupportAccountId { get; private set; }
    public SupportAccount SupportAccount { get; private set; }

    private Partner() { }

    public static Partner Create(
        PartnerId id,
        string code,
        string label,
        NetworkMode networkMode,
        PaymentMode paymentMode,
        string type,
        SupportAccountType supportAccountType,
        string taxIdentificationNumber,
        string taxRegime,
        string auxiliaryAccount,
        string ice,
        string rasRate,
        string logo,
        Guid? idParent = null,
        Guid? commissionAccountId = null,
        Guid? activityAccountId = null,
        Guid? supportAccountId = null)
    {
        var partner = new Partner
        {
            Id = id,
            Code = code,
            Label = label,
            NetworkMode = networkMode,
            PaymentMode = paymentMode,
            Type = type,
            SupportAccountType = supportAccountType,
            TaxIdentificationNumber = taxIdentificationNumber,
            TaxRegime = taxRegime,
            AuxiliaryAccount = auxiliaryAccount,
            ICE = ice,
            RASRate = rasRate,
            IsEnabled = true,
            Logo = logo,
            IdParent = idParent,
            CommissionAccountId = commissionAccountId,
            ActivityAccountId = activityAccountId,
            SupportAccountId = supportAccountId
        };

        // Raise the creation event
        partner.AddDomainEvent(new PartnerCreatedEvent(
            partner.Id.Value,
            partner.Code,
            partner.Label,
            partner.NetworkMode,
            partner.PaymentMode,
            String.Empty,
            partner.SupportAccountType,
            partner.TaxIdentificationNumber,
            partner.TaxRegime,
            partner.AuxiliaryAccount,
            partner.ICE,
            partner.IsEnabled,
            partner.Logo,
            DateTime.UtcNow
        ));

        return partner;
    }

    public void Update(
        string code,
        string label,
        NetworkMode networkMode,
        PaymentMode paymentMode,
        string type,
        SupportAccountType supportAccountType,
        string taxIdentificationNumber,
        string taxRegime,
        string auxiliaryAccount,
        string ice,
        string rasRate,
        string logo,
        Guid? idParent = null,
        Guid? commissionAccountId = null,
        Guid? activityAccountId = null,
        Guid? supportAccountId = null)
    {
        Code = code;
        Label = label;
        NetworkMode = networkMode;
        PaymentMode = paymentMode;
        Type = type;
        SupportAccountType = supportAccountType;
        TaxIdentificationNumber = taxIdentificationNumber;
        TaxRegime = taxRegime;
        AuxiliaryAccount = auxiliaryAccount;
        ICE = ice;
        RASRate = rasRate;
        Logo = logo;
        IdParent = idParent;
        CommissionAccountId = commissionAccountId;
        ActivityAccountId = activityAccountId;
        SupportAccountId = supportAccountId;

        // Raise the update event
        AddDomainEvent(new PartnerUpdatedEvent(
            Id.Value,
            Code,
            Label,
            NetworkMode,
            PaymentMode,
            String.Empty,
            SupportAccountType,
            TaxIdentificationNumber,
            TaxRegime,
            AuxiliaryAccount,
            ICE,
            IsEnabled,
            Logo,
            DateTime.UtcNow
        ));
    }

    public void Patch(
        string code,
        string label,
        NetworkMode networkMode,
        PaymentMode paymentMode,
        string type,
        SupportAccountType supportAccountType,
        string taxIdentificationNumber,
        string taxRegime,
        string auxiliaryAccount,
        string ice,
        string rasRate,
        string logo,
        Guid? idParent = null,
        Guid? commissionAccountId = null,
        Guid? activityAccountId = null,
        Guid? supportAccountId = null)
    {
        Code = code;
        Label = label;
        NetworkMode = networkMode;
        PaymentMode = paymentMode;
        Type = type;
        SupportAccountType = supportAccountType;
        TaxIdentificationNumber = taxIdentificationNumber;
        TaxRegime = taxRegime;
        AuxiliaryAccount = auxiliaryAccount;
        ICE = ice;
        RASRate = rasRate;
        Logo = logo;
        IdParent = idParent;
        CommissionAccountId = commissionAccountId;
        ActivityAccountId = activityAccountId;
        SupportAccountId = supportAccountId;

        // Raise the patch event
        AddDomainEvent(new PartnerPatchedEvent(
            Id.Value,
            Code,
            Label,
            NetworkMode,
            PaymentMode,
            String.Empty,
            SupportAccountType,
            TaxIdentificationNumber,
            TaxRegime,
            AuxiliaryAccount,
            ICE,
            IsEnabled,
            Logo,
            DateTime.UtcNow
        ));
    }

    public void Disable()
    {
        IsEnabled = false;

        // Raise the disable event
        AddDomainEvent(new PartnerDisabledEvent(
            Id.Value,
            DateTime.UtcNow
        ));
    }

    public void Activate()
    {
        IsEnabled = true;

        // Raise the activate event
        AddDomainEvent(new PartnerActivatedEvent(
            Id.Value,
            DateTime.UtcNow
        ));
    }

    // Account relationship methods
    public void SetCommissionAccount(Guid accountId, PartnerAccount account = null)
    {
        CommissionAccountId = accountId;
        if (account != null)
            CommissionAccount = account;
    }

    public void SetActivityAccount(Guid accountId, PartnerAccount account = null)
    {
        ActivityAccountId = accountId;
        if (account != null)
            ActivityAccount = account;
    }

    public void SetSupportAccount(Guid accountId, SupportAccount account = null)
    {
        SupportAccountId = accountId;
        if (account != null)
            SupportAccount = account;
    }
}