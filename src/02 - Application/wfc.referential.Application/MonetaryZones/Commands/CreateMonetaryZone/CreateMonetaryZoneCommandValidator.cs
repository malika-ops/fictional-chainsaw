using FluentValidation;


namespace wfc.referential.Application.MonetaryZones.Commands.CreateMonetaryZone;

public class CreateMonetaryZoneValidator : AbstractValidator<CreateMonetaryZoneCommand>
{

    public CreateMonetaryZoneValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .WithMessage("Code is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required");

        RuleFor(x => x.Description)
           .NotEmpty().WithMessage("Description is required");
    }

}

