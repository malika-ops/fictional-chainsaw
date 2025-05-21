namespace wfc.referential.Application.Taxes.Commands.UpdateTax;
using FluentValidation;

public class UpdateTaxCommandValidator : AbstractValidator<UpdateTaxCommand>
{
    public UpdateTaxCommandValidator()
    {
        RuleFor(x => x.TaxId)
            .NotEmpty().WithMessage(x => $"{nameof(x.TaxId)} is required.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage(x => $"{nameof(x.Code)} is required.")
            .MaximumLength(50).WithMessage(x => $"{nameof(x.Code)} must not exceed 50 characters.");

        RuleFor(x => x.CodeEn)
            .NotEmpty().WithMessage("English tax code is required.")
            .MaximumLength(100);

        RuleFor(x => x.CodeAr)
            .NotEmpty().WithMessage("Arabic tax code is required.")
            .MaximumLength(100);

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(250);

        RuleFor(t => t)
            .Must(x => (x.Rate > 0 || x.FixedAmount > 0))
            .WithMessage("Either Rate or FixedAmount must be provided or both.");

    }
}
