namespace wfc.referential.Application.TaxRuleDetails.Commands.UpdateTaxRuleDetail;
using FluentValidation;

public class UpdateTaxRuleDetailCommandValidator : AbstractValidator<UpdateTaxRuleDetailCommand>
{
    public UpdateTaxRuleDetailCommandValidator()
    {
        RuleFor(x => x.TaxRuleDetailsId)
            .NotEmpty().WithMessage($"{nameof(UpdateTaxRuleDetailCommand.TaxRuleDetailsId)} is required.");

        RuleFor(x => x.CorridorId)
            .NotEmpty().WithMessage($"{nameof(UpdateTaxRuleDetailCommand.CorridorId)} is required.");

        RuleFor(x => x.TaxId)
            .NotEmpty().WithMessage($"{nameof(UpdateTaxRuleDetailCommand.TaxId)} is required.");

        RuleFor(x => x.ServiceId)
            .NotEmpty().WithMessage($"{nameof(UpdateTaxRuleDetailCommand.ServiceId)} is required.");

        RuleFor(x => x.AppliedOn)
            .NotEmpty()
            .IsInEnum().WithMessage($"{nameof(UpdateTaxRuleDetailCommand.AppliedOn)} is required.");

        RuleFor(x => x.IsEnabled)
            .NotNull().WithMessage($"{nameof(UpdateTaxRuleDetailCommand.IsEnabled)} is required.");
    }
}