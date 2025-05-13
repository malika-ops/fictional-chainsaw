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
    public string? IdPartner { get; }
    public wfc.referential.Domain.SupportAccountAggregate.SupportAccountType? SupportAccountType { get; }
    public string? IdentificationNumber { get; }
    public string? TaxRegime { get; }
    public string? AuxiliaryAccount { get; }
    public string? ICE { get; }
    public bool? IsEnabled { get; }
    public string? Logo { get; }
    public Guid? SectorId { get; }
    public Guid? CityId { get; }

    public PatchPartnerCommand(
        Guid partnerId,
        string? code = null,
        string? label = null,
        NetworkMode? networkMode = null,
        PaymentMode? paymentMode = null,
        string? idPartner = null,
        wfc.referential.Domain.SupportAccountAggregate.SupportAccountType? supportAccountType = null,
        string? identificationNumber = null,
        string? taxRegime = null,
        string? auxiliaryAccount = null,
        string? ice = null,
        bool? isEnabled = null,
        string? logo = null,
        Guid? sectorId = null,
        Guid? cityId = null)
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