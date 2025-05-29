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

        // If name is provided, check not empty
        When(x => x.Name is not null, () => {
            RuleFor(x => x.Name!)
                .NotEmpty().WithMessage("Name cannot be empty if provided");
        });

        // If person type is provided, check not empty
        When(x => x.PersonType is not null, () => {
            RuleFor(x => x.PersonType!)
                .NotEmpty().WithMessage("PersonType cannot be empty if provided");
        });
    }
}