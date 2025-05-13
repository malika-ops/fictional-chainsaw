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
            RuleFor(x => x.Code!).NotEmpty().WithMessage("Code cannot be empty if provided."));

        When(x => x.Password is not null, () =>
            RuleFor(x => x.Password!).NotEmpty().WithMessage("Password cannot be empty if provided."));

        When(x => x.AgencyId is not null, () =>
            RuleFor(x => x.AgencyId!.Value).NotEqual(Guid.Empty));

        When(x => x.TierId is not null, () =>
            RuleFor(x => x.TierId!.Value).NotEqual(Guid.Empty));
    }
}