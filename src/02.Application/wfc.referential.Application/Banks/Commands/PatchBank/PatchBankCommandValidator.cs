using FluentValidation;

namespace wfc.referential.Application.Banks.Commands.PatchBank;

public class PatchBankCommandValidator : AbstractValidator<PatchBankCommand>
{
    public PatchBankCommandValidator()
    {
        RuleFor(x => x.BankId)
            .NotEqual(Guid.Empty).WithMessage("BankId cannot be empty.");

        // If code is provided (not null), check it's not empty
        When(x => x.Code is not null, () => {
            RuleFor(x => x.Code!)
                .NotEmpty().WithMessage("Code cannot be empty if provided.");
        });

        // If name is provided, check not empty
        When(x => x.Name is not null, () => {
            RuleFor(x => x.Name!)
                .NotEmpty().WithMessage("Name cannot be empty if provided.");
        });

        // If abbreviation is provided, check not empty
        When(x => x.Abbreviation is not null, () => {
            RuleFor(x => x.Abbreviation!)
                .NotEmpty().WithMessage("Abbreviation cannot be empty if provided.");
        });
    }
}