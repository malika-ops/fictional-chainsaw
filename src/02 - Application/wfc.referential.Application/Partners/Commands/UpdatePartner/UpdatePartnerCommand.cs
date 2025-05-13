using BuildingBlocks.Core.Abstraction.CQRS;
using wfc.referential.Domain.PartnerAggregate;

namespace wfc.referential.Application.Partners.Commands.UpdatePartner;

public record UpdatePartnerCommand : ICommand<Guid>
{
    public Guid PartnerId { get; }
    public string Code { get; }
    public string Label { get; }
    public NetworkMode NetworkMode { get; }
    public PaymentMode PaymentMode { get; }
    public string IdPartner { get; }
    public wfc.referential.Domain.SupportAccountAggregate.SupportAccountType SupportAccountType { get; }
    public string IdentificationNumber { get; }
    public string TaxRegime { get; }
    public string AuxiliaryAccount { get; }
    public string ICE { get; }
    public bool IsEnabled { get; }
    public string Logo { get; }
    public Guid SectorId { get; }
    public Guid CityId { get; }

    public UpdatePartnerCommand(
        Guid partnerId,
        string code,
        string label,
        NetworkMode networkMode,
        PaymentMode paymentMode,
        string idPartner,
        wfc.referential.Domain.SupportAccountAggregate.SupportAccountType supportAccountType,
        string identificationNumber,
        string taxRegime,
        string auxiliaryAccount,
        string ice,
        bool isEnabled,
        string logo,
        Guid sectorId,
        Guid cityId)
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
    }
}