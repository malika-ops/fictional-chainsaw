using FluentValidation;

namespace wfc.referential.Application.Banks.Commands.DeleteBank;

public class DeleteBankCommandValidator : AbstractValidator<DeleteBankCommand>
{
    public DeleteBankCommandValidator()
    {
        RuleFor(x => x.BankId)
            .NotEqual(Guid.Empty)
            .WithMessage("BankId must be a non-empty GUID.");
    }
}