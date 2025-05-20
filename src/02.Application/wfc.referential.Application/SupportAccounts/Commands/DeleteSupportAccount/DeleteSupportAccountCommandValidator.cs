using FluentValidation;

namespace wfc.referential.Application.SupportAccounts.Commands.DeleteSupportAccount;

public class DeleteSupportAccountCommandValidator : AbstractValidator<DeleteSupportAccountCommand>
{
    public DeleteSupportAccountCommandValidator()
    {
        // Ensure the ID is non-empty & parseable as a GUID
        RuleFor(x => x.SupportAccountId)
            .NotEmpty()
            .WithMessage("SupportAccountId must be a non-empty GUID.");
    }
}