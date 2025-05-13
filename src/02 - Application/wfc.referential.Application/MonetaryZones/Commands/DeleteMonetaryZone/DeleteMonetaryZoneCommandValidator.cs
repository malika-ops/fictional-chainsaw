using FluentValidation;


namespace wfc.referential.Application.MonetaryZones.Commands.DeleteMonetaryZone;

public class DeleteMonetaryZoneCommandValidator : AbstractValidator<DeleteMonetaryZoneCommand>
{

    public DeleteMonetaryZoneCommandValidator()
    {

        // ensure the ID is non-empty & parseable as a GUID
        RuleFor(x => x.MonetaryZoneId)
            .NotEmpty()
            .WithMessage("MonetaryZoneId must be a non-empty GUID.");
    }
}
