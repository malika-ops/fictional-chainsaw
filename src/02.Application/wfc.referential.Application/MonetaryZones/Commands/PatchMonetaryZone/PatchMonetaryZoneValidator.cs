using FluentValidation;

namespace wfc.referential.Application.MonetaryZones.Commands.PatchMonetaryZone;

public class PatchMonetaryZoneValidator : AbstractValidator<PatchMonetaryZoneCommand>
{
    public PatchMonetaryZoneValidator()
    {
        // If code is provided (not null), check it's not empty
        When(x => x.Code is not null, () => {
            RuleFor(x => x.Code!)
                .NotEmpty()
                .WithMessage("Code cannot be empty if provided.")
                .MaximumLength(50).WithMessage("Code must be less than 50 characters");
        });

        // If name is provided, check not empty, etc.
        When(x => x.Name is not null, () => {
            RuleFor(x => x.Name!)
            .NotEmpty()
            .WithMessage("Name cannot be empty if provided.")
            .MaximumLength(100).WithMessage("Name must be less than 100 characters");
        });

        When(x => x.Description is not null, () => {
            RuleFor(x => x.Description!)
            .MaximumLength(500).WithMessage("Description must be less than 500 characters");
        });
    }
}
