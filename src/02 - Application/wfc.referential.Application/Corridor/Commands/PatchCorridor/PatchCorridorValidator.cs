using FluentValidation;

namespace wfc.referential.Application.Corridors.Commands.PatchCorridor;

public class PatchCorridorValidator : AbstractValidator<PatchCorridorCommand>
{

    public PatchCorridorValidator()
    {

        When(c => c.SourceCountryId is not null, () => {
            RuleFor(c => c.SourceCountryId!)
                .NotEmpty()
                .WithMessage(c => $"{c.SourceCountryId} cannot be empty if provided.");
        });
        When(c => c.DestinationCountryId is not null, () => {
            RuleFor(c => c.DestinationCountryId!)
                .NotEmpty()
                .WithMessage(c => $"{c.DestinationCountryId} cannot be empty if provided.");
        });
        When(c => c.SourceCityId is not null, () => {
            RuleFor(c => c.SourceCityId!)
                .NotEmpty()
                .WithMessage(c => $"{c.SourceCityId} cannot be empty if provided.");
        });
        When(c => c.DestinationCityId is not null, () => {
            RuleFor(c => c.DestinationCityId!)
                .NotEmpty()
                .WithMessage(c => $"{c.SourceCityId} cannot be empty if provided.");
        });
        When(c => c.SourceAgencyId is not null, () => {
            RuleFor(c => c.SourceAgencyId!)
                .NotEmpty()
                .WithMessage(c => $"{c.SourceAgencyId} cannot be empty if provided.");
        });
        When(c => c.DestinationAgencyId is not null, () => {
            RuleFor(c => c.DestinationAgencyId!)
                .NotEmpty()
                .WithMessage(c => $"{c.DestinationAgencyId} cannot be empty if provided.");
        });
    }
}
