using FluentValidation;

namespace wfc.referential.Application.Corridors.Commands.UpdateCorridor;

public class UpdateCorridorValidator : AbstractValidator<UpdateCorridorCommand>
{
    public UpdateCorridorValidator()
    {
        RuleFor(c => c.CorridorId)
            .NotEqual(Guid.Empty).WithMessage(c => $"{c.CorridorId} cannot be empty.");

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
