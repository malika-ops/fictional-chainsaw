using FluentValidation;


namespace wfc.referential.Application.MonetaryZones.Commands.CreateMonetaryZone;

public class CreateMonetaryZoneValidator : AbstractValidator<CreateMonetaryZoneCommand>
{

    public CreateMonetaryZoneValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required")
            .MaximumLength(20).WithMessage("Code must be less than 10 characters");


        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name must be less than 100 characters");
    }

}

