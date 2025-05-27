using FluentValidation;

namespace wfc.referential.Application.Tiers.Commands.CreateTier;

public class CreateTierCommandValidator : AbstractValidator<CreateTierCommand>
{
    public CreateTierCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name max length is 100 chars.");
    }
}