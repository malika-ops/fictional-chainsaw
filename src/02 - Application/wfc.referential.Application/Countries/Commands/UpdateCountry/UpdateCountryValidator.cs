using FluentValidation;

namespace wfc.referential.Application.Countries.Commands.UpdateCountry;

public class UpdateCountryValidator : AbstractValidator<UpdateCountryCommand>
{
    public UpdateCountryValidator()
    {

        RuleFor(x => x.CountryId)
                .NotEqual(Guid.Empty)
                .WithMessage("CountryId cannot be empty.");

        RuleFor(x => x.Code)
            .NotEmpty()
            .WithMessage("Code is required.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required.");

        RuleFor(x => x.Abbreviation)
            .NotEmpty()
            .WithMessage("Abbreviation is required.");

        RuleFor(x => x.ISO2)
            .NotEmpty()
            .WithMessage("ISO2 is required.");

        RuleFor(x => x.ISO3)
            .NotEmpty()
            .WithMessage("ISO3 is required.");

        RuleFor(x => x.DialingCode)
            .NotEmpty()
            .WithMessage("Dialing Code is required.");

        RuleFor(x => x.TimeZone)
            .NotEmpty()
            .WithMessage("Time Zone is required.");

        RuleFor(x => x.NumberDecimalDigits)
            .NotEmpty()
            .WithMessage("Number of Decimal Digits is required.")
            .InclusiveBetween(0, 10)
            .WithMessage("NumberDecimalDigits must be between 0 and 10.");

        RuleFor(x => x.HasSector)
            .NotNull()
            .WithMessage("Has Sector is required.");

        RuleFor(x => x.IsSmsEnabled)
            .NotNull()
            .WithMessage("Is SMS Enabled is required.");

        RuleFor(x => x.MonetaryZoneId)
            .NotEqual(Guid.Empty)
            .WithMessage("MonetaryZoneId cannot be empty.");
    }
}
