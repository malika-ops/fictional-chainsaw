using FluentValidation;

namespace wfc.referential.Application.CountryServices.Commands.CreateCountryService;

public class CreateCountryServiceCommandValidator : AbstractValidator<CreateCountryServiceCommand>
{
    public CreateCountryServiceCommandValidator()
    {
        RuleFor(x => x.CountryId)
            .NotEmpty().WithMessage("CountryId is required");

        RuleFor(x => x.ServiceId)
            .NotEmpty().WithMessage("ServiceId is required");
    }
}