using FluentValidation;

namespace wfc.referential.Application.AgencyTiers.Commands.PatchAgencyTier;

internal class PatchAgencyTierValidator : AbstractValidator<PatchAgencyTierCommand>
{
    public PatchAgencyTierValidator()
    {
        RuleFor(x => x.AgencyTierId)
            .NotEqual(Guid.Empty)
            .WithMessage("AgencyTierId cannot be empty.");

        When(x => x.Code is not null, () =>
            RuleFor(x => x.Code!).NotEmpty().WithMessage("Code cannot be empty if provided.")
            .MaximumLength(30).WithMessage("Code max length = 30."));
    }
}