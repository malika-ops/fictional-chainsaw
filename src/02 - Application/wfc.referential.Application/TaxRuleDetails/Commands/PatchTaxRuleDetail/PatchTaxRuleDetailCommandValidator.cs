using FluentValidation;

namespace wfc.referential.Application.TaxRuleDetails.Commands.PatchTaxRuleDetail;

public class PatchTaxRuleDetailCommandValidator : AbstractValidator<PatchTaxRuleDetailCommand>
{
    public PatchTaxRuleDetailCommandValidator()
    {
        RuleFor(x => x.TaxRuleDetailsId)
            .NotEmpty().WithMessage($"{nameof(PatchTaxRuleDetailCommand.TaxRuleDetailsId)} is required.");

        When(x => x.AppliedOn is not null, () =>
        {
            RuleFor(x => x.AppliedOn)
                .IsInEnum().WithMessage($"{nameof(PatchTaxRuleDetailCommand.AppliedOn)} cannot be empty if provided.");
        });

        When(x => x.ServiceId is not null, () =>
        {
            RuleFor(x => x.ServiceId)
                .NotEmpty().WithMessage($"{nameof(PatchTaxRuleDetailCommand.ServiceId)} cannot be empty if provided.");
        });

        When(x => x.CorridorId is not null, () =>
        {
            RuleFor(x => x.CorridorId)
                .NotEmpty().WithMessage($"{nameof(PatchTaxRuleDetailCommand.CorridorId)} cannot be empty if provided.");
        });

        When(x => x.TaxId is not null, () =>
        {
            RuleFor(x => x.TaxId)
                .NotEmpty().WithMessage($"{nameof(PatchTaxRuleDetailCommand.TaxId)} cannot be empty if provided.");
        });

        When(x => x.IsEnabled.HasValue, () =>
        {
            RuleFor(x => x.IsEnabled)
                .NotEmpty().WithMessage($"{nameof(PatchTaxRuleDetailCommand.IsEnabled)} is required.");
        });
    }

}
