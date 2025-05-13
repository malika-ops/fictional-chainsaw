using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Domain.PartnerAggregate.Events;
using wfc.referential.Domain.SectorAggregate;
using wfc.referential.Domain.CityAggregate;
using wfc.referential.Domain.SupportAccountAggregate;

namespace wfc.referential.Domain.PartnerAggregate;

public class Partner : Aggregate<PartnerId>
{
    public string Code { get; private set; } = string.Empty;
    public string Label { get; private set; } = string.Empty;
    public NetworkMode NetworkMode { get; private set; }
    public PaymentMode PaymentMode { get; private set; }
    public string IdPartner { get; private set; } = string.Empty;
    public SupportAccountType SupportAccountType { get; private set; }
    public string IdentificationNumber { get; private set; } = string.Empty;
    public string TaxRegime { get; private set; } = string.Empty;
    public string AuxiliaryAccount { get; private set; } = string.Empty;
    public string ICE { get; private set; } = string.Empty;
    public bool IsEnabled { get; private set; } = true;
    public string Logo { get; private set; } = string.Empty;

    // Relations
    public Sector Sector { get; private set; }
    public SectorId SectorId { get; private set; }
    public City City { get; private set; }
    public CityId CityId { get; private set; }

    private Partner() { }

    public static Partner Create(
        PartnerId id,
        string code,
        string label,
        NetworkMode networkMode,
        PaymentMode paymentMode,
        string idPartner,
        SupportAccountType supportAccountType,
        string identificationNumber,
        string taxRegime,
        string auxiliaryAccount,
        string ice,
        string logo,
        Sector sector,
        City city)
    {
        var partner = new Partner
        {
            Id = id,
            Code = code,
            Label = label,
            NetworkMode = networkMode,
            PaymentMode = paymentMode,
            IdPartner = idPartner,
            SupportAccountType = supportAccountType,
            IdentificationNumber = identificationNumber,
            TaxRegime = taxRegime,
            AuxiliaryAccount = auxiliaryAccount,
            ICE = ice,
            IsEnabled = true,
            Logo = logo,
            Sector = sector,
            SectorId = sector.Id,
            City = city,
            CityId = city.Id
        };

        // Raise the creation event
        partner.AddDomainEvent(new PartnerCreatedEvent(
            partner.Id.Value,
            partner.Code,
            partner.Label,
            partner.NetworkMode,
            partner.PaymentMode,
            partner.IdPartner,
            partner.SupportAccountType,
            partner.IdentificationNumber,
            partner.TaxRegime,
            partner.AuxiliaryAccount,
            partner.ICE,
            partner.IsEnabled,
            partner.Logo,
            partner.SectorId.Value,
            partner.CityId.Value,
            DateTime.UtcNow
        ));

        return partner;
    }

    public void Update(
        string code,
        string label,
        NetworkMode networkMode,
        PaymentMode paymentMode,
        string idPartner,
        SupportAccountType supportAccountType,
        string identificationNumber,
        string taxRegime,
        string auxiliaryAccount,
        string ice,
        string logo,
        Sector sector,
        City city)
    {
        Code = code;
        Label = label;
        NetworkMode = networkMode;
        PaymentMode = paymentMode;
        IdPartner = idPartner;
        SupportAccountType = supportAccountType;
        IdentificationNumber = identificationNumber;
        TaxRegime = taxRegime;
        AuxiliaryAccount = auxiliaryAccount;
        ICE = ice;
        Logo = logo;
        Sector = sector;
        SectorId = sector.Id;
        City = city;
        CityId = city.Id;

        // Raise the update event
        AddDomainEvent(new PartnerUpdatedEvent(
            Id.Value,
            Code,
            Label,
            NetworkMode,
            PaymentMode,
            IdPartner,
            SupportAccountType,
            IdentificationNumber,
            TaxRegime,
            AuxiliaryAccount,
            ICE,
            IsEnabled,
            Logo,
            SectorId.Value,
            CityId.Value,
            DateTime.UtcNow
        ));
    }

    public void Patch(
        string code,
        string label,
        NetworkMode networkMode,
        PaymentMode paymentMode,
        string idPartner,
        SupportAccountType supportAccountType,
        string identificationNumber,
        string taxRegime,
        string auxiliaryAccount,
        string ice,
        string logo,
        Sector sector,
        City city)
    {
        Code = code;
        Label = label;
        NetworkMode = networkMode;
        PaymentMode = paymentMode;
        IdPartner = idPartner;
        SupportAccountType = supportAccountType;
        IdentificationNumber = identificationNumber;
        TaxRegime = taxRegime;
        AuxiliaryAccount = auxiliaryAccount;
        ICE = ice;
        Logo = logo;
        Sector = sector;
        SectorId = sector.Id;
        City = city;
        CityId = city.Id;

        // Raise the patch event
        AddDomainEvent(new PartnerPatchedEvent(
            Id.Value,
            Code,
            Label,
            NetworkMode,
            PaymentMode,
            IdPartner,
            SupportAccountType,
            IdentificationNumber,
            TaxRegime,
            AuxiliaryAccount,
            ICE,
            IsEnabled,
            Logo,
            SectorId.Value,
            CityId.Value,
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
}