using FluentValidation;

namespace wfc.referential.Application.Banks.Commands.DeleteBank;

public class DeleteBankCommandValidator : AbstractValidator<DeleteBankCommand>
{
    public DeleteBankCommandValidator()
    {
        // Ensure the ID is non-empty & parseable as a GUID
        RuleFor(x => x.BankId)
            .NotEmpty()
            .WithMessage("BankId must be a non-empty GUID.");
    }
}