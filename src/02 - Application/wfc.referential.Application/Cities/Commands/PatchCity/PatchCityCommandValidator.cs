using FluentValidation;

namespace wfc.referential.Application.Cities.Commands.PatchCity;

public class PatchCityCommandValidator : AbstractValidator<PatchCityCommand>
{

    public PatchCityCommandValidator()
    {

        // If code is provided (not null), check it's not empty
        When(x => x.Code is not null, () => {
            RuleFor(x => x.Code!)
                .NotEmpty()
                .WithMessage("Code cannot be empty if provided.");
        });

        // If name is provided, check not empty, etc.
        When(x => x.Name is not null, () => {
            RuleFor(x => x.Name!)
            .NotEmpty()
            .WithMessage("Name cannot be empty if provided.");
        });

        // If description is provided
        When(x => x.IsEnabled is not null, () => {
            RuleFor(x => x.IsEnabled!)
            .NotEmpty()
            .WithMessage("IsEnabled cannot be empty if provided.");
        });

        When(x => x.Abbreviation is not null, () => {
            RuleFor(x => x.Abbreviation!)
            .NotEmpty()
            .WithMessage("Abbreviation cannot be empty if provided.");
        });
        When(x => x.TaxZone is not null, () => {
            RuleFor(x => x.TaxZone!)
            .NotEmpty()
            .WithMessage("TaxZone cannot be empty if provided.");
        });
        When(x => x.TimeZone is not null, () => {
            RuleFor(x => x.TimeZone!)
            .NotEmpty()
            .WithMessage("TimeZone cannot be empty if provided.");
        });
        When(x => x.RegionId is not null, () => {
            RuleFor(x => x.RegionId!)
            .NotEmpty()
            .WithMessage("RegionId cannot be empty if provided.");
        });
    }
}
