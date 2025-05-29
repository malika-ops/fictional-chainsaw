using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.Partners.Commands.PatchPartner;

public record PatchPartnerCommand : ICommand<Result<bool>>
{
    public Guid PartnerId { get; init; }
    public string? Code { get; init; }
    public string? Name { get; init; }
    public string? PersonType { get; init; }
    public string? ProfessionalTaxNumber { get; init; }
    public string? WithholdingTaxRate { get; init; }
    public string? HeadquartersCity { get; init; }
    public string? HeadquartersAddress { get; init; }
    public string? LastName { get; init; }
    public string? FirstName { get; init; }
    public string? PhoneNumberContact { get; init; }
    public string? MailContact { get; init; }
    public string? FunctionContact { get; init; }
    public string? TransferType { get; init; }
    public string? AuthenticationMode { get; init; }
    public string? TaxIdentificationNumber { get; init; }
    public string? TaxRegime { get; init; }
    public string? AuxiliaryAccount { get; init; }
    public string? ICE { get; init; }
    public string? Logo { get; init; }
    public bool? IsEnabled { get; init; }
    public Guid? NetworkModeId { get; init; }
    public Guid? PaymentModeId { get; init; }
    public Guid? PartnerTypeId { get; init; }
    public Guid? SupportAccountTypeId { get; init; }
    public Guid? CommissionAccountId { get; init; }
    public Guid? ActivityAccountId { get; init; }
    public Guid? SupportAccountId { get; init; }
    public Guid? IdParent { get; init; }
}