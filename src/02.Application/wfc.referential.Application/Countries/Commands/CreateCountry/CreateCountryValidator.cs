using FluentValidation;

namespace wfc.referential.Application.Countries.Commands.CreateCountry;

public class CreateCountryValidator : AbstractValidator<CreateCountryCommand>
{
    public CreateCountryValidator()
    {

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Country name is required.");

        RuleFor(x => x.Code)
            .NotEmpty()
            .WithMessage("Country code is required.");


        RuleFor(x => x.ISO2)
            .NotEmpty().WithMessage("ISO2 code is required.")
            .Length(2).WithMessage("ISO2 code must be exactly 2 characters.");

        RuleFor(x => x.ISO3)
            .NotEmpty().WithMessage("ISO3 code is required.")
            .Length(3).WithMessage("ISO3 code must be exactly 3 characters.");

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
            .NotNull()
            .WithMessage("Number of decimal digits is required.")
            .InclusiveBetween(1, 10)
            .WithMessage("NumberDecimalDigits must be between 1 and 10.");

        RuleFor(x => x.MonetaryZoneId)
            .NotEmpty()
            .WithMessage("MonetaryZoneId is required.");

        RuleFor(x => x.CurrencyId)
            .NotEmpty()
            .WithMessage("CurrencyId is required.");
    }
}