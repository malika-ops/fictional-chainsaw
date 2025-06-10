using FluentValidation;

namespace wfc.referential.Application.CountryServices.Commands.UpdateCountryService;

public class UpdateCountryServiceCommandValidator : AbstractValidator<UpdateCountryServiceCommand>
{
    public UpdateCountryServiceCommandValidator()
    {
        RuleFor(x => x.CountryServiceId)
            .NotEqual(Guid.Empty).WithMessage("CountryServiceId cannot be empty");

        RuleFor(x => x.CountryId)
            .NotEmpty().WithMessage("CountryId is required");

        RuleFor(x => x.ServiceId)
            .NotEmpty().WithMessage("ServiceId is required");
    }
}