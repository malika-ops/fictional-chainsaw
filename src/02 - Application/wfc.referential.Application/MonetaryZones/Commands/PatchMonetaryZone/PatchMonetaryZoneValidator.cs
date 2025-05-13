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
                .WithMessage("Code cannot be empty if provided.");
        });

        // If name is provided, check not empty, etc.
        When(x => x.Name is not null, () => {
            RuleFor(x => x.Name!)
            .NotEmpty()
            .WithMessage("Name cannot be empty if provided.");
        });

        // If description is provided
        When(x => x.Description is not null, () => {
            RuleFor(x => x.Description!)
            .NotEmpty()
            .WithMessage("Description cannot be empty if provided.");
        });
    }
}
