using FluentValidation;

namespace wfc.referential.Application.Corridors.Commands.CreateCorridor;


public class CreateCorridorCommandValidator : AbstractValidator<CreateCorridorCommand>
{

    public CreateCorridorCommandValidator()
    {

        RuleFor(x => x.SourceCountryId)
            .NotEmpty().WithMessage("SourceCountryId is required");
        RuleFor(x => x.DestinationCountryId)
            .NotEmpty().WithMessage("DestinationCountryId is required");
    }

}