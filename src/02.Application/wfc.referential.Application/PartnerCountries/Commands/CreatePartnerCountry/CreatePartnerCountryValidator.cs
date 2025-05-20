using FluentValidation;

namespace wfc.referential.Application.PartnerCountries.Commands.CreatePartnerCountry;

public class CreatePartnerCountryValidator : AbstractValidator<CreatePartnerCountryCommand>
{
    public CreatePartnerCountryValidator()
    {
        RuleFor(x => x.PartnerId)
            .NotEqual(Guid.Empty).WithMessage("PartnerId is required.");

        RuleFor(x => x.CountryId)
            .NotEqual(Guid.Empty).WithMessage("CountryId is required.");
    }
}