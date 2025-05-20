using BuildingBlocks.Core.Abstraction.CQRS;
using wfc.referential.Domain.PartnerAggregate;

namespace wfc.referential.Application.Partners.Commands.PatchPartner;

public record PatchPartnerCommand : ICommand<Guid>
{
    // The ID from the route
    public Guid PartnerId { get; }

    // The optional fields to update
    public string? Code { get; }
    public string? Label { get; }
    public NetworkMode? NetworkMode { get; }
    public PaymentMode? PaymentMode { get; }
    public string? Type { get; }
    public wfc.referential.Domain.SupportAccountAggregate.SupportAccountType? SupportAccountType { get; }
    public string? IdentificationNumber { get; }
    public string? TaxRegime { get; }
    public string? AuxiliaryAccount { get; }
    public string? ICE { get; }
    public string? RASRate { get; }
    public bool? IsEnabled { get; }
    public string? Logo { get; }
    public Guid? IdParent { get; }
    public Guid? CommissionAccountId { get; }
    public Guid? ActivityAccountId { get; }
    public Guid? SupportAccountId { get; }

    public PatchPartnerCommand(
        Guid partnerId,
        string? code = null,
        string? label = null,
        NetworkMode? networkMode = null,
        PaymentMode? paymentMode = null,
        string? type = null,
        wfc.referential.Domain.SupportAccountAggregate.SupportAccountType? supportAccountType = null,
        string? identificationNumber = null,
        string? taxRegime = null,
        string? auxiliaryAccount = null,
        string? ice = null,
        string? rasRate = null,
        bool? isEnabled = null,
        string? logo = null,
        Guid? idParent = null,
        Guid? commissionAccountId = null,
        Guid? activityAccountId = null,
        Guid? supportAccountId = null)
    {
        PartnerId = partnerId;
        Code = code;
        Label = label;
        NetworkMode = networkMode;
        PaymentMode = paymentMode;
        Type = type;
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