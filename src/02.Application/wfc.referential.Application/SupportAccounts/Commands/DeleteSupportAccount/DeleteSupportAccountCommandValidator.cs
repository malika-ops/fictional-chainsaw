using FluentValidation;

namespace wfc.referential.Application.SupportAccounts.Commands.DeleteSupportAccount;

public class DeleteSupportAccountCommandValidator : AbstractValidator<DeleteSupportAccountCommand>
{
    public DeleteSupportAccountCommandValidator()
    {
        RuleFor(x => x.SupportAccountId)
            .NotEqual(Guid.Empty)
            .WithMessage("SupportAccountId must be a non-empty GUID.");
    }
}