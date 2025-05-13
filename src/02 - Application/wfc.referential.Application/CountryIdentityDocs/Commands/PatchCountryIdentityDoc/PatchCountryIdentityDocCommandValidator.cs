using FluentValidation;

namespace wfc.referential.Application.CountryIdentityDocs.Commands.PatchCountryIdentityDoc;

public class PatchCountryIdentityDocCommandValidator : AbstractValidator<PatchCountryIdentityDocCommand>
{
    public PatchCountryIdentityDocCommandValidator()
    {
        RuleFor(x => x.CountryIdentityDocId)
            .NotEqual(Guid.Empty).WithMessage("CountryIdentityDocId cannot be empty");

        // Si CountryId est fourni, vérifier qu'il n'est pas vide
        When(x => x.CountryId.HasValue, () => {
            RuleFor(x => x.CountryId!.Value)
                .NotEqual(Guid.Empty).WithMessage("CountryId cannot be empty if provided");
        });

        // Si IdentityDocumentId est fourni, vérifier qu'il n'est pas vide
        When(x => x.IdentityDocumentId.HasValue, () => {
            RuleFor(x => x.IdentityDocumentId!.Value)
                .NotEqual(Guid.Empty).WithMessage("IdentityDocumentId cannot be empty if provided");
        });
    }
}