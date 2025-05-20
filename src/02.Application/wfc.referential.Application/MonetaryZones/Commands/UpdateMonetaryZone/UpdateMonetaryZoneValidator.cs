using FluentValidation;

namespace wfc.referential.Application.MonetaryZones.Commands.UpdateMonetaryZone;

public class UpdateMonetaryZoneValidator : AbstractValidator<UpdateMonetaryZoneCommand>
{
    public UpdateMonetaryZoneValidator()
    {
        // 1) Ensure ID is not empty
        RuleFor(x => x.MonetaryZoneId)
            .NotEqual(Guid.Empty).WithMessage("MonetaryZoneId cannot be empty.");

        // 3) Code not empty
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required")
            .MaximumLength(20).WithMessage("Code must be less than 10 characters");

        // 3) Name not empty
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name must be less than 100 characters");
    }
}
