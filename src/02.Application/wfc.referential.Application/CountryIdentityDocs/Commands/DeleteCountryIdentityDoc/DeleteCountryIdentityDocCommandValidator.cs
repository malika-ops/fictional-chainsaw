using FluentValidation;

namespace wfc.referential.Application.CountryIdentityDocs.Commands.DeleteCountryIdentityDoc;

public class DeleteCountryIdentityDocCommandValidator : AbstractValidator<DeleteCountryIdentityDocCommand>
{
    public DeleteCountryIdentityDocCommandValidator()
    {
        // Assurer que l'ID n'est pas vide et parsable en GUID
        RuleFor(x => x.CountryIdentityDocId)
            .NotEmpty()
            .WithMessage("CountryIdentityDocId must be a non-empty GUID.");
    }
}