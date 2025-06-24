using FluentValidation;

namespace wfc.referential.Application.Pricings.Commands.PatchPricing;

public class PatchPricingValidator : AbstractValidator<PatchPricingCommand>
{
    public PatchPricingValidator()
    {
        RuleFor(x => x.PricingId)
            .NotEqual(Guid.Empty).WithMessage("PricingId cannot be empty.");

        When(x => x.Code is not null, () =>
        {
            RuleFor(x => x.Code!)
                .NotEmpty().WithMessage("Code cannot be empty if provided.")
                .MaximumLength(50).WithMessage("Code max length = 50.");
        });

        When(x => x.Channel is not null, () =>
        {
            RuleFor(x => x.Channel!)
                .NotEmpty().WithMessage("Channel cannot be empty if provided.")
                .MaximumLength(50).WithMessage("Channel max length = 50.");
        });

        When(x => x.MinimumAmount.HasValue, () =>
            RuleFor(x => x.MinimumAmount!.Value)
                .GreaterThan(0).WithMessage("MinimumAmount must be positive."));

        When(x => x.MaximumAmount.HasValue, () =>
            RuleFor(x => x.MaximumAmount!.Value)
                .GreaterThan(0).WithMessage("MaximumAmount must be positive."));

        When(x => x.FixedAmount.HasValue, () =>
            RuleFor(x => x.FixedAmount!.Value)
                .GreaterThan(0).WithMessage("FixedAmount must be positive."));

        When(x => x.Rate.HasValue, () =>
            RuleFor(x => x.Rate!.Value)
                .GreaterThan(0).WithMessage("Rate must be positive."));

        RuleFor(x => x)
            .Must(x => !(x.MinimumAmount.HasValue && x.MaximumAmount.HasValue) ||
                       x.MaximumAmount > x.MinimumAmount)
            .WithMessage("MaximumAmount must be strictly greater than MinimumAmount.");
    }
}