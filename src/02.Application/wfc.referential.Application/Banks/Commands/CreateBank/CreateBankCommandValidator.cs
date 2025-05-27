using FluentValidation;

namespace wfc.referential.Application.Banks.Commands.CreateBank;

public class CreateBankCommandValidator : AbstractValidator<CreateBankCommand>
{
    public CreateBankCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty()
            .WithMessage("Bank code is required.");
        RuleFor(x => x.Name).NotEmpty()
            .WithMessage("Bank Name is required.");
        RuleFor(x => x.Abbreviation).NotEmpty()
            .WithMessage("Abbreviation is required.");
    }
}