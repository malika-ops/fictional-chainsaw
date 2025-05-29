using FluentValidation;

namespace wfc.referential.Application.Partners.Commands.CreatePartner;

public class CreatePartnerCommandValidator : AbstractValidator<CreatePartnerCommand>
{
    public CreatePartnerCommandValidator()
    {
        // Existing mandatory fields
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required");

        RuleFor(x => x.PersonType)
            .NotEmpty().WithMessage("PersonType is required");

        // Additional mandatory fields based on user story
        RuleFor(x => x.TaxIdentificationNumber)
            .NotEmpty().WithMessage("Tax Identification Number is required");

        RuleFor(x => x.ProfessionalTaxNumber)
            .NotEmpty().WithMessage("Professional Tax Number is required");

        RuleFor(x => x.ICE)
            .NotEmpty().WithMessage("ICE is required");

        // Conditional validations based on PersonType
        RuleFor(x => x.WithholdingTaxRate)
            .NotEmpty().WithMessage("Withholding Tax Rate is required")
            .When(x => x.PersonType == "Natural Person/Legal Person" || x.PersonType == "Natural Person");

        RuleFor(x => x.HeadquartersCity)
            .NotEmpty().WithMessage("Headquarters City is required")
            .When(x => !string.IsNullOrEmpty(x.PersonType));

        RuleFor(x => x.HeadquartersAddress)
            .NotEmpty().WithMessage("Headquarters Address is required")
            .When(x => !string.IsNullOrEmpty(x.PersonType));

        // Contact fields - at least one should be provided
        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last Name Contact is required")
            .When(x => !string.IsNullOrEmpty(x.PersonType));

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First Name Contact is required")
            .When(x => !string.IsNullOrEmpty(x.PersonType));

        RuleFor(x => x.PhoneNumberContact)
            .NotEmpty().WithMessage("Phone Number contact is required")
            .When(x => !string.IsNullOrEmpty(x.PersonType));

        RuleFor(x => x.MailContact)
            .NotEmpty().WithMessage("Mail contact is required")
            .When(x => !string.IsNullOrEmpty(x.PersonType));

        RuleFor(x => x.FunctionContact)
            .NotEmpty().WithMessage("Function contact is required")
            .When(x => !string.IsNullOrEmpty(x.PersonType));

        // Logo is optional but should be validated if provided
        RuleFor(x => x.Logo)
            .NotEmpty().WithMessage("Logo is required")
            .When(x => !string.IsNullOrEmpty(x.PersonType));

        // Authentication mode validation
        RuleFor(x => x.AuthenticationMode)
            .NotEmpty().WithMessage("Authentication mode is required");

        // Conditional validations for PartnerType = 'réseau propre'
        RuleFor(x => x.PaymentModeId)
            .NotNull().WithMessage("Payment Mode is required when PartnerType is 'réseau propre'");

        RuleFor(x => x.TransferType)
            .NotEmpty().WithMessage("Transfer Type is required when PartnerType is 'réseau propre'");

        RuleFor(x => x.ActivityAccountId)
            .NotNull().WithMessage("Activity Account is required when PartnerType is 'réseau propre'");

        RuleFor(x => x.CommissionAccountId)
            .NotNull().WithMessage("Commission Account is required when PartnerType is 'réseau propre'");

        // Conditional validation for Network mode = 'prépayé'
        RuleFor(x => x.SupportAccountTypeId)
            .NotNull().WithMessage("Support Account Type is required when Network mode is 'prépayé'");

        RuleFor(x => x.SupportAccountId)
            .NotNull().WithMessage("Support Account ID is required when Network mode is 'prépayé' and Support Account Type is 'commun'");
    }
}