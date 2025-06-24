using FluentValidation;

namespace wfc.referential.Application.Affiliates.Commands.DeleteAffiliate;

public class DeleteAffiliateCommandValidator : AbstractValidator<DeleteAffiliateCommand>
{
    public DeleteAffiliateCommandValidator()
    {
        RuleFor(x => x.AffiliateId)
            .NotEqual(Guid.Empty).WithMessage("AffiliateId must be a non-empty GUID.");
    }
}