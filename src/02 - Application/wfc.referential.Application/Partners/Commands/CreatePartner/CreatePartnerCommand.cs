using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Domain.PartnerAggregate;

namespace wfc.referential.Application.Partners.Commands.CreatePartner;

public record CreatePartnerCommand : ICommand<Result<Guid>>
{
    public string Code { get; }
    public string Label { get; }
    public NetworkMode NetworkMode { get; }
    public string Type { get; }
    public PaymentMode PaymentMode { get; }
    public wfc.referential.Domain.SupportAccountAggregate.SupportAccountType SupportAccountType { get; }
    public string IdentificationNumber { get; }
    public string TaxRegime { get; }
    public string AuxiliaryAccount { get; }
    public string ICE { get; }
    public string RASRate { get; }
    public string Logo { get; }
    public Guid? IdParent { get; }
    public Guid? CommissionAccountId { get; }
    public Guid? ActivityAccountId { get; }
    public Guid? SupportAccountId { get; }

    public CreatePartnerCommand(
        string code,
        string label,
        NetworkMode networkMode,
        string type,
        PaymentMode paymentMode,
        wfc.referential.Domain.SupportAccountAggregate.SupportAccountType supportAccountType,
        string identificationNumber,
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
        Type = type;
        PaymentMode = paymentMode;
        SupportAccountType = supportAccountType;
        IdentificationNumber = identificationNumber;
        TaxRegime = taxRegime;
        AuxiliaryAccount = auxiliaryAccount;
        ICE = ice;
        RASRate = rasRate;
        Logo = logo;
        IdParent = idParent;
        CommissionAccountId = commissionAccountId;
        ActivityAccountId = activityAccountId;
        SupportAccountId = supportAccountId;
    }
}