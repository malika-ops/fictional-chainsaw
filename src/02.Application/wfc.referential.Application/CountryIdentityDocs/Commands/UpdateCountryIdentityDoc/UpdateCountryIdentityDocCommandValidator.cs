using FluentValidation;

namespace wfc.referential.Application.CountryIdentityDocs.Commands.UpdateCountryIdentityDoc;

public class UpdateCountryIdentityDocCommandValidator : AbstractValidator<UpdateCountryIdentityDocCommand>
{
    public UpdateCountryIdentityDocCommandValidator()
    {
        RuleFor(x => x.CountryIdentityDocId)
            .NotEqual(Guid.Empty).WithMessage("CountryIdentityDocId cannot be empty");

        RuleFor(x => x.CountryId)
            .NotEmpty().WithMessage("CountryId is required");

        RuleFor(x => x.IdentityDocumentId)
            .NotEmpty().WithMessage("IdentityDocumentId is required");
    }
}