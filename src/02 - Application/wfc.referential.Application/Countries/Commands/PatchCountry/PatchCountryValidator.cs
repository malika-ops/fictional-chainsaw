using FluentValidation;

namespace wfc.referential.Application.Countries.Commands.PatchCountry;

public class PatchCountryValidator : AbstractValidator<PatchCountryCommand>
{
    public PatchCountryValidator()
    {

        When(x => x.Abbreviation is not null, () =>
            RuleFor(x => x.Abbreviation!).NotEmpty());

        When(x => x.Name is not null, () =>
            RuleFor(x => x.Name!).NotEmpty());

        When(x => x.Code is not null, () =>
            RuleFor(x => x.Code!).NotEmpty());

        When(x => x.ISO2 is not null, () =>
            RuleFor(x => x.ISO2!).NotEmpty());

        When(x => x.ISO3 is not null, () =>
            RuleFor(x => x.ISO3!).NotEmpty());

        When(x => x.DialingCode is not null, () =>
            RuleFor(x => x.DialingCode!).NotEmpty());

        When(x => x.TimeZone is not null, () =>
            RuleFor(x => x.TimeZone!).NotEmpty());

        When(x => x.NumberDecimalDigits is not null, () =>
            RuleFor(x => x.NumberDecimalDigits!).InclusiveBetween(0, 10)
                .WithMessage("NumberDecimalDigits must be between 0 and 10."));

        When(x => x.IsSmsEnabled is not null, () =>
            RuleFor(x => x.IsSmsEnabled!).NotNull());
        
        When(x => x.HasSector is not null, () =>
            RuleFor(x => x.HasSector!).NotNull());
    }
}
