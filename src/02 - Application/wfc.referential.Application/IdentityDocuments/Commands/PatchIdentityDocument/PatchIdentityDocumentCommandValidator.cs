using FluentValidation;

namespace wfc.referential.Application.IdentityDocuments.Commands.PatchIdentityDocument;

public class PatchIdentityDocumentCommandValidator : AbstractValidator<PatchIdentityDocumentCommand>
{
    public PatchIdentityDocumentCommandValidator()
    {
        RuleFor(x => x.IdentityDocumentId).NotEmpty();

        // Add validation for Code if it's provided (not null)
        When(x => x.Code != null, () => {
            RuleFor(x => x.Code).NotEmpty().WithMessage("Code cannot be empty");
        });

        // Add validation for Name if it's provided (not null)
        When(x => x.Name != null, () => {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Name cannot be empty");
        });
    }
}