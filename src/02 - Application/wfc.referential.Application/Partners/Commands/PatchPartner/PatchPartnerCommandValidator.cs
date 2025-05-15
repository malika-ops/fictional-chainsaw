using FluentValidation;

namespace wfc.referential.Application.Partners.Commands.PatchPartner;

public class PatchPartnerCommandValidator : AbstractValidator<PatchPartnerCommand>
{
    public PatchPartnerCommandValidator()
    {
        RuleFor(x => x.PartnerId)
           .NotEqual(Guid.Empty).WithMessage("PartnerId cannot be empty");

        // If code is provided, check not empty
        When(x => x.Code is not null, () => {
            RuleFor(x => x.Code!)
                .NotEmpty().WithMessage("Code cannot be empty if provided");
        });

        // If label is provided, check not empty
        When(x => x.Label is not null, () => {
            RuleFor(x => x.Label!)
                .NotEmpty().WithMessage("Label cannot be empty if provided");
        });

        // If type is provided, check not empty
        When(x => x.Type is not null, () => {
            RuleFor(x => x.Type!)
                .NotEmpty().WithMessage("Type cannot be empty if provided");
        });

        // If IdParent is provided, check not empty
        When(x => x.IdParent.HasValue, () => {
            RuleFor(x => x.IdParent!.Value)
                .NotEqual(Guid.Empty).WithMessage("IdParent cannot be empty if provided");
        });

        // If CommissionAccountId is provided, check not empty
        When(x => x.CommissionAccountId.HasValue, () => {
            RuleFor(x => x.CommissionAccountId!.Value)
                .NotEqual(Guid.Empty).WithMessage("CommissionAccountId cannot be empty if provided");
        });

        // If ActivityAccountId is provided, check not empty
        When(x => x.ActivityAccountId.HasValue, () => {
            RuleFor(x => x.ActivityAccountId!.Value)
                .NotEqual(Guid.Empty).WithMessage("ActivityAccountId cannot be empty if provided");
        });

        // If SupportAccountId is provided, check not empty
        When(x => x.SupportAccountId.HasValue, () => {
            RuleFor(x => x.SupportAccountId!.Value)
                .NotEqual(Guid.Empty).WithMessage("SupportAccountId cannot be empty if provided");
        });
    }
}