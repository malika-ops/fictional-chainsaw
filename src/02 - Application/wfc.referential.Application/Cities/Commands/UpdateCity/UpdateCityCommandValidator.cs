using FluentValidation;

namespace wfc.referential.Application.Cities.Commands.UpdateCity;

public class UpdateCityCommandValidator : AbstractValidator<UpdateCityCommand>
{
    public UpdateCityCommandValidator()
    {
        RuleFor(x => x.CityId)
            .NotEqual(Guid.Empty).WithMessage("CityId cannot be empty.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required");

        RuleFor(x => x.IsEnabled)
            .NotEmpty().WithMessage("IsEnabled is required");

        RuleFor(x => x.Abbreviation)
            .NotEmpty().WithMessage("Abbreviation is required");

        RuleFor(x => x.TimeZone)
            .NotEmpty().WithMessage("TimeZone is required");

        RuleFor(x => x.TaxZone)
            .NotEmpty().WithMessage("TaxZone is required");

        RuleFor(x => x.RegionId!)
        .NotEmpty().WithMessage("RegionId cannot be empty if provided.");
    }
}
