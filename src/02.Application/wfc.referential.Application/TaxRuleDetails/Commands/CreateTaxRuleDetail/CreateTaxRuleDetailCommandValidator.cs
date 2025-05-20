using FluentValidation;

namespace wfc.referential.Application.TaxRuleDetails.Commands.CreateTaxRuleDetail;


public class CreateTaxRuleDetailCommandValidator : AbstractValidator<CreateTaxRuleDetailCommand>
{
    public CreateTaxRuleDetailCommandValidator()
    {
        RuleFor(x => x.CorridorId)
            .NotEmpty().WithMessage($"{nameof(CreateTaxRuleDetailCommand.CorridorId)} is required.");

        RuleFor(x => x.TaxId)
            .NotEmpty().WithMessage($"{nameof(CreateTaxRuleDetailCommand.TaxId)} is required.");

        RuleFor(x => x.AppliedOn)
            .IsInEnum().WithMessage($"{nameof(CreateTaxRuleDetailCommand.AppliedOn)} is required.");

        RuleFor(x => x.ServiceId)
            .NotEmpty().WithMessage($"{nameof(CreateTaxRuleDetailCommand.ServiceId)} is required.");
    }
}
