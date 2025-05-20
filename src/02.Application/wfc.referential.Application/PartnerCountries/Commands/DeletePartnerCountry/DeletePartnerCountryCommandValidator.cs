using FluentValidation;

namespace wfc.referential.Application.PartnerCountries.Commands.DeletePartnerCountry;

public class DeletePartnerCountryCommandValidator : AbstractValidator<DeletePartnerCountryCommand>
{
    public DeletePartnerCountryCommandValidator()
    {
        RuleFor(x => x.PartnerCountryId)
            .NotEqual(Guid.Empty)
            .WithMessage("PartnerCountryId must be a non-empty GUID.");
    }
}