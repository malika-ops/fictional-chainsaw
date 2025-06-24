using FluentValidation;

namespace wfc.referential.Application.Countries.Commands.PatchCountry;

public class PatchCountryValidator : AbstractValidator<PatchCountryCommand>
{
    public PatchCountryValidator()
    {

        When(x => x.Abbreviation is not null, () =>
            RuleFor(x => x.Abbreviation!).NotEmpty());

        When(x => x.Name is not null, () =>
            RuleFor(x => x.Name!).NotEmpty().MaximumLength(100));

        When(x => x.Code is not null, () =>
            RuleFor(x => x.Code!).NotEmpty().MaximumLength(50));

        When(x => x.ISO2 is not null, () =>
            RuleFor(x => x.ISO2!)
            .NotEmpty()
            .Length(2)
            .WithMessage("ISO2 code must be exactly 2 characters."));

        When(x => x.ISO3 is not null, () =>
            RuleFor(x => x.ISO3!)
            .NotEmpty()
            .Length(3)
            .WithMessage("ISO3 code must be exactly 3 characters."));

        When(x => x.DialingCode is not null, () =>
            RuleFor(x => x.DialingCode!).NotEmpty().MaximumLength(4));

        When(x => x.TimeZone is not null, () =>
            RuleFor(x => x.TimeZone!).NotEmpty());

        When(x => x.NumberDecimalDigits is not null, () =>
            RuleFor(x => x.NumberDecimalDigits!).InclusiveBetween(1, 10)
                .WithMessage("NumberDecimalDigits must be between 1 and 10."));

        When(x => x.IsSmsEnabled is not null, () =>
            RuleFor(x => x.IsSmsEnabled!).NotNull());
        
        When(x => x.HasSector is not null, () =>
            RuleFor(x => x.HasSector!).NotNull());
    }
}
