using FluentValidation;

namespace wfc.referential.Application.Pricings.Commands.CreatePricing;

public class CreatePricingValidator : AbstractValidator<CreatePricingCommand>
{
    public CreatePricingValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required.")
            .MaximumLength(50).WithMessage("Code max length = 50.");

        RuleFor(x => x.Channel)
            .NotEmpty().WithMessage("Channel is required.")
            .MaximumLength(50).WithMessage("Channel max length = 50.");

        RuleFor(x => x.MinimumAmount)
            .GreaterThan(0).WithMessage("MinimumAmount must be positive.");

        RuleFor(x => x.MaximumAmount)
            .GreaterThan(x => x.MinimumAmount)
            .WithMessage("MaximumAmount must be strictly greater than MinimumAmount.");

        RuleFor(x => new { x.FixedAmount, x.Rate })
            .Must(f => f.FixedAmount.HasValue || f.Rate.HasValue)
            .WithMessage("Either FixedAmount or Rate must be provided (or both).");

        When(x => x.FixedAmount.HasValue, () =>
        {
            RuleFor(x => x.FixedAmount!)
            .GreaterThan(0)
            .WithMessage("FixedAmount must be positive.");
        });

        When(x => x.Rate.HasValue, () =>
        {
            RuleFor(x => x.Rate!)
            .GreaterThan(0)
            .WithMessage("Rate must be positive.");
        });

        RuleFor(x => x.ServiceId)
            .NotEmpty().WithMessage("ServiceId cannot be empty.");

        RuleFor(x => x.CorridorId)
            .NotEmpty().WithMessage("CorridorId cannot be empty.");
    }
}