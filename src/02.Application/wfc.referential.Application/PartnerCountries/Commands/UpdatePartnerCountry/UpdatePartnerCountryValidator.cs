using FluentValidation;

namespace wfc.referential.Application.PartnerCountries.Commands.UpdatePartnerCountry;

public class UpdatePartnerCountryValidator : AbstractValidator<UpdatePartnerCountryCommand>
{
    public UpdatePartnerCountryValidator()
    {
        RuleFor(x => x.PartnerCountryId).NotEqual(Guid.Empty);
        RuleFor(x => x.PartnerId).NotEqual(Guid.Empty);
        RuleFor(x => x.CountryId).NotEqual(Guid.Empty);
    }
}