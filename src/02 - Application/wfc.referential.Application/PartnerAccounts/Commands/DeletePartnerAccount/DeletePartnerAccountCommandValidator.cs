using FluentValidation;

namespace wfc.referential.Application.PartnerAccounts.Commands.DeletePartnerAccount;

public class DeletePartnerAccountCommandValidator : AbstractValidator<DeletePartnerAccountCommand>
{
    public DeletePartnerAccountCommandValidator()
    {
        // Ensure the ID is non-empty & parseable as a GUID
        RuleFor(x => x.PartnerAccountId)
            .NotEmpty()
            .WithMessage("PartnerAccountId must be a non-empty GUID.");
    }
}