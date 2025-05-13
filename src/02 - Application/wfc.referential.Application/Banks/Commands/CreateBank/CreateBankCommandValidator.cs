using FluentValidation;

namespace wfc.referential.Application.Banks.Commands.CreateBank;

public class CreateBankCommandValidator : AbstractValidator<CreateBankCommand>
{
    public CreateBankCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required");

        RuleFor(x => x.Abbreviation)
            .NotEmpty().WithMessage("Abbreviation is required");
    }
}