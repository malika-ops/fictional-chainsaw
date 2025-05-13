using FluentValidation;

namespace wfc.referential.Application.Agencies.Commands.CreateAgency;

public class CreateAgencyValidator : AbstractValidator<CreateAgencyCommand>
{
    public CreateAgencyValidator()
    {

        RuleFor(x => x.Code).NotEmpty()
            .WithMessage("Agency code is required.");
        RuleFor(x => x.Name).NotEmpty()
            .WithMessage("Agency Name is required.");
        RuleFor(x => x.Abbreviation).NotEmpty()
            .WithMessage("Abbreviation is required.");
        RuleFor(x => x.Address1).NotEmpty()
            .WithMessage("Address is required.");
        RuleFor(x => x.Phone).NotEmpty()
            .WithMessage("Phone number is required.");
        RuleFor(x => x.AccountingSheetName).NotEmpty()
            .WithMessage("Accounting sheet name is required.");
        RuleFor(x => x.AccountingAccountNumber).NotEmpty()
            .WithMessage("Accounting account number is required.");
        RuleFor(x => x.PostalCode).NotEmpty()
            .WithMessage("Postal code is required.");

    }
}