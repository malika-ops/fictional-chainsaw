using FluentValidation;


namespace wfc.referential.Application.TaxRuleDetails.Commands.DeleteTaxRuleDetail;

public class DeleteTaxRuleDetailCommandValidator : AbstractValidator<DeleteTaxRuleDetailCommand>
{

    public DeleteTaxRuleDetailCommandValidator()
    {
        RuleFor(x => x.TaxRuleDetailsId)
            .NotEmpty().WithMessage($"{nameof(DeleteTaxRuleDetailCommand.TaxRuleDetailsId)} is required.");
    }
}
