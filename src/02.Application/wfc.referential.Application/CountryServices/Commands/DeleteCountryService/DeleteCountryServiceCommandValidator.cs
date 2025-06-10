using FluentValidation;

namespace wfc.referential.Application.CountryServices.Commands.DeleteCountryService;

public class DeleteCountryServiceCommandValidator : AbstractValidator<DeleteCountryServiceCommand>
{
    public DeleteCountryServiceCommandValidator()
    {
        // Assurer que l'ID n'est pas vide et parsable en GUID
        RuleFor(x => x.CountryServiceId)
            .NotEmpty()
            .WithMessage("CountryServiceId must be a non-empty GUID.");
    }
}