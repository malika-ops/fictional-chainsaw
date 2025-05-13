using FluentValidation;

namespace wfc.referential.Application.Countries.Commands.CreateCountry;

public class CreateCountryValidator : AbstractValidator<CreateCountryCommand>
{
    public CreateCountryValidator()
    {
        RuleFor(x => x.Abbreviation)
            .NotEmpty()
            .WithMessage("Abbreviation is required.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Country name is required.");

        RuleFor(x => x.Code)
            .NotEmpty()
            .WithMessage("Country code is required.");

        RuleFor(x => x.ISO2)
            .NotEmpty()
            .WithMessage("ISO2 code is required.");

        RuleFor(x => x.ISO3)
            .NotEmpty()
            .WithMessage("ISO3 code is required.");

        RuleFor(x => x.DialingCode)
            .NotEmpty()
            .WithMessage("Dialing code is required.");

        RuleFor(x => x.TimeZone)
            .NotEmpty()
            .WithMessage("Time zone is required.");

        RuleFor(x => x.HasSector)
            .NotNull()
            .WithMessage("HasSector is required.");

        RuleFor(x => x.IsSmsEnabled)
            .NotNull()
            .WithMessage("IsSmsEnabled is required.");

        RuleFor(x => x.NumberDecimalDigits)
            .NotEmpty()
            .WithMessage("Number of decimal digits is required.")
            .InclusiveBetween(0, 10)
            .WithMessage("NumberDecimalDigits must be between 0 and 10.");

        RuleFor(x => x.MonetaryZoneId)
            .NotEmpty()
            .WithMessage("MonetaryZoneId is required.");
    }
}