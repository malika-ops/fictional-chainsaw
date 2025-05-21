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
        
        RuleFor(t => t)
            .Must(x => (x.Rate.HasValue || x.FixedAmount.HasValue))
            .WithMessage("Either Rate or FixedAmount must be provided or both.");

    }
}
