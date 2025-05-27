using FluentValidation;

namespace wfc.referential.Application.Tiers.Commands.UpdateTier;

public class UpdateTierValidator : AbstractValidator<UpdateTierCommand>
{
    public UpdateTierValidator()
    {
        RuleFor(x => x.TierId)
            .NotEqual(Guid.Empty)
            .WithMessage("TierId cannot be empty.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name max length is 100 chars.");
    }
}