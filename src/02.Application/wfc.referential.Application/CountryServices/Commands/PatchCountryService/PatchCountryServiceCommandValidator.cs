using FluentValidation;

namespace wfc.referential.Application.CountryServices.Commands.PatchCountryService;

public class PatchCountryServiceCommandValidator : AbstractValidator<PatchCountryServiceCommand>
{
    public PatchCountryServiceCommandValidator()
    {
        RuleFor(x => x.CountryServiceId)
            .NotEqual(Guid.Empty).WithMessage("CountrySerivceId cannot be empty");

        When(x => x.CountryId.HasValue, () => {
            RuleFor(x => x.CountryId!.Value)
                .NotEqual(Guid.Empty).WithMessage("CountryId cannot be empty if provided");
        });

        When(x => x.ServiceId.HasValue, () => {
            RuleFor(x => x.ServiceId!.Value)
                .NotEqual(Guid.Empty).WithMessage("ServiceId cannot be empty if provided");
        });
    }
}