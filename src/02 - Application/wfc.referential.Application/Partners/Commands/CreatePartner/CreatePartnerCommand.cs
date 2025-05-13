using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Domain.PartnerAggregate;

namespace wfc.referential.Application.Partners.Commands.CreatePartner;

public record CreatePartnerCommand : ICommand<Result<Guid>>
{
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
    public string Logo { get; }
    public Guid SectorId { get; }
    public Guid CityId { get; }

    public CreatePartnerCommand(
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
        string logo,
        Guid sectorId,
        Guid cityId)
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
        SectorId = sectorId;
        CityId = cityId;
    }
}