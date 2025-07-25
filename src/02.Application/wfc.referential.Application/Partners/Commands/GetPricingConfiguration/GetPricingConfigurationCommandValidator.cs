using FluentValidation;

namespace wfc.referential.Application.Partners.Commands.GetPricingConfiguration;

public class GetPricingConfigurationCommandValidator
    : AbstractValidator<GetPricingConfigurationCommand>
{
    public GetPricingConfigurationCommandValidator()
    {
        RuleFor(x => x.PartnerId)
            .NotEmpty().WithMessage("PartnerId is required.");
        RuleFor(x => x.ServiceId)
            .NotEmpty().WithMessage("ServiceId is required.");
        RuleFor(x => x.CorridorId)
            .NotEmpty().WithMessage("CorridorId is required.");
        RuleFor(x => x.AffiliateId)
            .NotEmpty().WithMessage("AffiliateId is required.");
        RuleFor(x => x.Channel)
            .NotEmpty().WithMessage("Channel is required.")
            .MaximumLength(50).WithMessage("Channel must not exceed 50 characters.");
        RuleFor(x => x.Amount)
            .GreaterThan(0m).WithMessage("Amount must be greater than zero.");
    }
}