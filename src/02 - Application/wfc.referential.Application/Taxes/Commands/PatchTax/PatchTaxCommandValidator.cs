using FluentValidation;

namespace wfc.referential.Application.Taxes.Commands.PatchTax;

public class PatchTaxCommandValidator : AbstractValidator<PatchTaxCommand>
{
    public PatchTaxCommandValidator()
    {
        RuleFor(x => x.TaxId)
            .NotEmpty().WithMessage(x => $"{nameof(x.TaxId)} is required.");

        When(x => x.Code is not null, () =>
        {
            RuleFor(x => x.Code)
                .NotEmpty().WithMessage(x => $"{nameof(x.Code)} cannot be empty if provided.")
                .MaximumLength(50).WithMessage(x => $"Tax {nameof(x.Code)} must not exceed 50 characters.");
        });

        When(x => x.CodeEn is not null, () =>
        {
            RuleFor(x => x.CodeEn)
                .NotEmpty().WithMessage(x => $"{nameof(x.CodeEn)} cannot be empty if provided.")
                .MaximumLength(100)
                .WithMessage(x => $"{nameof(x.CodeEn)} must not exceed 100 characters.");
        });

        When(x => !string.IsNullOrWhiteSpace(x.CodeAr), () =>
        {
            RuleFor(x => x.CodeAr)
                .NotEmpty().WithMessage(x => $"{nameof(x.CodeAr)} cannot be empty if provided.")
                .MaximumLength(100).WithMessage(x => $"{nameof(x.CodeAr)} must not exceed 100 characters.");
        });

        When(x => !string.IsNullOrWhiteSpace(x.Description), () =>
        {
            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("CodeEn cannot be empty if provided.")
                .MaximumLength(250).WithMessage(x => $"{nameof(x.Description)} must not exceed 250 characters.");
        });

        When(x => x.Value.HasValue, () =>
        {
            RuleFor(x => x.Value.Value)
                .NotEmpty().WithMessage(x => $"{x.Value} cannot be empty if provided.")
                .GreaterThanOrEqualTo(0).WithMessage(x => $"{x.Value} must be non-negative.")
                .LessThanOrEqualTo(100).WithMessage(x => $"{x.Value} must not exceed 100%.");
        });


        When(x => x.IsEnabled.HasValue, () =>
        {
            RuleFor(x => x.IsEnabled)
                .NotEmpty().WithMessage(x => $"{nameof(x.IsEnabled)} is required");
        });
    }
}
