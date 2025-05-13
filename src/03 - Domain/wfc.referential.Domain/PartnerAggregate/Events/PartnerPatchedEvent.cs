using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Domain.SupportAccountAggregate;

namespace wfc.referential.Domain.PartnerAggregate.Events;

public record PartnerPatchedEvent : IDomainEvent
{
    public Guid PartnerId { get; }
    public string Code { get; }
    public string Label { get; }
    public NetworkMode NetworkMode { get; }
    public PaymentMode PaymentMode { get; }
    public string IdPartner { get; }
    public SupportAccountType SupportAccountType { get; }
    public string IdentificationNumber { get; }
    public string TaxRegime { get; }
    public string AuxiliaryAccount { get; }
    public string ICE { get; }
    public bool IsEnabled { get; }
    public string Logo { get; }
    public Guid SectorId { get; }
    public Guid CityId { get; }
    public DateTime OccurredOn { get; }

    public PartnerPatchedEvent(
        Guid partnerId,
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
        bool isEnabled,
        string logo,
        Guid sectorId,
        Guid cityId,
        DateTime occurredOn)
    {
        PartnerId = partnerId;
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
        IsEnabled = isEnabled;
        Logo = logo;
        SectorId = sectorId;
        CityId = cityId;
        OccurredOn = occurredOn;
    }
}