using FluentValidation;
using wfc.referential.Application.Data;

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
            .NotEmpty().WithMessage("Code is required");

        // 3) Name not empty
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required");

        // 4) Description not empty
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required");
    }
}
