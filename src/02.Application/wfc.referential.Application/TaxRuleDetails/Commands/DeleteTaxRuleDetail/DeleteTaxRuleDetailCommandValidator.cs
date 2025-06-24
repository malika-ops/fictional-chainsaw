using FluentValidation;


namespace wfc.referential.Application.TaxRuleDetails.Commands.DeleteTaxRuleDetail;

public class DeleteTaxRuleDetailCommandValidator : AbstractValidator<DeleteTaxRuleDetailCommand>
{

    public DeleteTaxRuleDetailCommandValidator()
    {
        RuleFor(x => x.TaxRuleDetailId)
            .NotEmpty().WithMessage($"{nameof(DeleteTaxRuleDetailCommand.TaxRuleDetailId)} is required.");
    }
}
