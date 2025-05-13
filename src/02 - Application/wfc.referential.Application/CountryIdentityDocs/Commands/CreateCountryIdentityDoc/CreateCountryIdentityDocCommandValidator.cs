using FluentValidation;

namespace wfc.referential.Application.CountryIdentityDocs.Commands.CreateCountryIdentityDoc;

public class CreateCountryIdentityDocCommandValidator : AbstractValidator<CreateCountryIdentityDocCommand>
{
    public CreateCountryIdentityDocCommandValidator()
    {
        RuleFor(x => x.CountryId)
            .NotEmpty().WithMessage("CountryId is required");

        RuleFor(x => x.IdentityDocumentId)
            .NotEmpty().WithMessage("IdentityDocumentId is required");
    }
}