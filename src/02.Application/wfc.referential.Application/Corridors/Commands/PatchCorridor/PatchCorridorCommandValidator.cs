using FluentValidation;

namespace wfc.referential.Application.Corridors.Commands.PatchCorridor;

public class PatchCorridorCommandValidator : AbstractValidator<PatchCorridorCommand>
{

    public PatchCorridorCommandValidator()
    {

        When(c => c.SourceCountryId is not null, () => {
            RuleFor(c => c.SourceCountryId!)
                .Must(id => id.Value != Guid.Empty)
                .WithMessage(c => $"{c.SourceCountryId} cannot be empty if provided.");
        });
        When(c => c.DestinationCountryId is not null, () => {
            RuleFor(c => c.DestinationCountryId!)
                .Must(id => id.Value != Guid.Empty)
                .WithMessage(c => $"{c.DestinationCountryId} cannot be empty if provided.");
        });
        When(c => c.SourceCityId is not null, () => {
            RuleFor(c => c.SourceCityId!)
                .Must(id => id.Value != Guid.Empty)
                .WithMessage(c => $"{c.SourceCityId} cannot be empty if provided.");
        });
        When(c => c.DestinationCityId is not null, () => {
            RuleFor(c => c.DestinationCityId!)
                .Must(id => id.Value != Guid.Empty)
                .WithMessage(c => $"{c.SourceCityId} cannot be empty if provided.");
        });
        When(c => c.SourceBranchId is not null, () => {
            RuleFor(c => c.SourceBranchId!)
                .NotEmpty()
                .WithMessage(c => $"{c.SourceBranchId} cannot be empty if provided.");
        });
        When(c => c.DestinationBranchId is not null, () => {
            RuleFor(c => c.DestinationBranchId!)
                .Must(id => id.Value != Guid.Empty)
                .WithMessage(c => $"{c.DestinationBranchId} cannot be empty if provided.");
        });
    }
}
