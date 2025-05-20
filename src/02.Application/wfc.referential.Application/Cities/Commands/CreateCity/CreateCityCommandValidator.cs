using FluentValidation;

namespace wfc.referential.Application.Cities.Commands.CreateCity;

public class CreateCityCommandValidator : AbstractValidator<CreateCityCommand>
{
    public CreateCityCommandValidator()
    {

        RuleFor(x => x.CityCode)
            .NotEmpty().WithMessage("Code is required");

        RuleFor(x => x.CityName)
            .NotEmpty().WithMessage("Name is required");

        RuleFor(x => x.IsEnabled)
            .NotEmpty().WithMessage("IsEnabled is required");

        RuleFor(x => x.TimeZone)
            .NotEmpty().WithMessage("TimeZone is required");

        RuleFor(x => x.RegionId!)
        .NotEmpty().WithMessage("RegionId is required");
    }
}
