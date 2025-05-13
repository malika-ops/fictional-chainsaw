using FluentValidation;

namespace wfc.referential.Application.Taxes.Commands.CreateTax;


public class CreateTaxCommandValidator : AbstractValidator<CreateTaxCommand>
{
    public CreateTaxCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage(x => $"{nameof(x.Code)} is required");

        RuleFor(x => x.CodeEn)
            .NotEmpty().WithMessage(x => $"{nameof(x.CodeEn)} Code is required");

        RuleFor(x => x.CodeAr)
            .NotEmpty().WithMessage(x => $"{nameof(x.CodeAr)} Code is required");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage(x => $"{nameof(x.Description)} is required");

        RuleFor(x => x.FixedAmount)
            .NotEmpty().WithMessage(x => $"{nameof(x.FixedAmount)} is required");

        RuleFor(x => x.Value)
            .GreaterThanOrEqualTo(0).WithMessage(x => $"{nameof(x.Value)} must be zero or greater");

    }
}
