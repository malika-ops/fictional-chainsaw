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
            .MaximumLength(200).WithMessage("Name max length is 200 chars.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.");
    }
}