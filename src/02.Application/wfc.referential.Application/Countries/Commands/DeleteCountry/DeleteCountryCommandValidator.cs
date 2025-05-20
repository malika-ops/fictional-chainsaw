using FluentValidation;

namespace wfc.referential.Application.Countries.Commands.DeleteCountry;

public class DeleteCountryCommandValidator : AbstractValidator<DeleteCountryCommand>
{
    public DeleteCountryCommandValidator()
    {
        RuleFor(x => x.CountryId)
            .NotEqual(Guid.Empty)
            .WithMessage("CountryId must be a non-empty GUID.");
    }
}
