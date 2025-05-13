using FluentValidation;


namespace wfc.referential.Application.Taxes.Commands.DeleteTax;

public class DeleteTaxCommandValidator : AbstractValidator<DeleteTaxCommand>
{

    public DeleteTaxCommandValidator()
    {
        RuleFor(x => x.TaxId)
            .NotEmpty().WithMessage("TaxId is required.");
    }
}
