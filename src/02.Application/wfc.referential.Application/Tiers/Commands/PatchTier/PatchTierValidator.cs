using FluentValidation;

namespace wfc.referential.Application.Tiers.Commands.PatchTier;

public class PatchTierValidator : AbstractValidator<PatchTierCommand>
{
    public PatchTierValidator()
    {
        RuleFor(x => x.TierId)
            .NotEqual(Guid.Empty)
            .WithMessage("TierId cannot be empty.");

        When(x => x.Name is not null, () =>
            RuleFor(x => x.Name!)
                .NotEmpty().WithMessage("Name cannot be empty when provided.")
                .MaximumLength(200));

        When(x => x.Description is not null, () =>
            RuleFor(x => x.Description!)
                .NotEmpty().WithMessage("Description cannot be empty when provided."));
    }
}