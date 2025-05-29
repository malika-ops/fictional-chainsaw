using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.Partners.Commands.UpdatePartner;

public record UpdatePartnerCommand : ICommand<Result<bool>>
{
    public Guid PartnerId { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string PersonType { get; init; } = string.Empty;
    public string ProfessionalTaxNumber { get; init; } = string.Empty;
    public string WithholdingTaxRate { get; init; } = string.Empty;
    public string HeadquartersCity { get; init; } = string.Empty;
    public string HeadquartersAddress { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string PhoneNumberContact { get; init; } = string.Empty;
    public string MailContact { get; init; } = string.Empty;
    public string FunctionContact { get; init; } = string.Empty;
    public string TransferType { get; init; } = string.Empty;
    public string AuthenticationMode { get; init; } = string.Empty;
    public string TaxIdentificationNumber { get; init; } = string.Empty;
    public string TaxRegime { get; init; } = string.Empty;
    public string AuxiliaryAccount { get; init; } = string.Empty;
    public string ICE { get; init; } = string.Empty;
    public string Logo { get; init; } = string.Empty;
    public bool IsEnabled { get; init; } = true;
    public Guid? NetworkModeId { get; init; }
    public Guid? PaymentModeId { get; init; }
    public Guid? PartnerTypeId { get; init; }
    public Guid? SupportAccountTypeId { get; init; }
    public Guid? CommissionAccountId { get; init; }
    public Guid? ActivityAccountId { get; init; }
    public Guid? SupportAccountId { get; init; }
    public Guid? IdParent { get; init; }
}