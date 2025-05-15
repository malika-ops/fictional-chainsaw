using BuildingBlocks.Core.Abstraction.CQRS;
using wfc.referential.Domain.PartnerAggregate;

namespace wfc.referential.Application.Partners.Commands.UpdatePartner;

public record UpdatePartnerCommand : ICommand<Guid>
{
    public Guid PartnerId { get; }
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
    public bool IsEnabled { get; }
    public string Logo { get; }
    public Guid? IdParent { get; }
    public Guid? CommissionAccountId { get; }
    public Guid? ActivityAccountId { get; }
    public Guid? SupportAccountId { get; }

    public UpdatePartnerCommand(
        Guid partnerId,
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
        bool isEnabled,
        string logo,
        Guid? idParent = null,
        Guid? commissionAccountId = null,
        Guid? activityAccountId = null,
        Guid? supportAccountId = null)
    {
        PartnerId = partnerId;
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
        IsEnabled = isEnabled;
        Logo = logo;
        IdParent = idParent;
        CommissionAccountId = commissionAccountId;
        ActivityAccountId = activityAccountId;
        SupportAccountId = supportAccountId;
    }
}