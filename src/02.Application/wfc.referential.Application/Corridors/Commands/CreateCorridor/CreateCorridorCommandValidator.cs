using FluentValidation;

namespace wfc.referential.Application.Corridors.Commands.CreateCorridor;


public class CreateCorridorCommandValidator : AbstractValidator<CreateCorridorCommand>
{

    public CreateCorridorCommandValidator()
    {

        RuleFor(command => command)
            .Must(command => 
                command.SourceCountryId.HasValue ||
                command.DestinationCountryId.HasValue ||
                command.SourceCityId.HasValue ||
                command.DestinationCityId.HasValue ||
                command.SourceBranchId.HasValue ||
                command.DestinationBranchId.HasValue)
            .WithMessage("At least one of the source or destination fields must be provided.");
    }

}