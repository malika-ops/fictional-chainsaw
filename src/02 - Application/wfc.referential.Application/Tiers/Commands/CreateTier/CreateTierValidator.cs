using FluentValidation;

namespace wfc.referential.Application.Tiers.Commands.CreateTier;

public class CreateTierValidator : AbstractValidator<CreateTierCommand>
{
    public CreateTierValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name max length is 200 chars.");
    }
}