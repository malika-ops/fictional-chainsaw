using FluentValidation;

namespace wfc.referential.Application.IdentityDocuments.Commands.PatchIdentityDocument;

public class PatchIdentityDocumentCommandValidator : AbstractValidator<PatchIdentityDocumentCommand>
{
    public PatchIdentityDocumentCommandValidator()
    {
        RuleFor(x => x.IdentityDocumentId)
            .NotEqual(Guid.Empty).WithMessage("IdentityDocumentId cannot be empty.");

        // If code is provided (not null), check it's not empty
        When(x => x.Code is not null, () => {
            RuleFor(x => x.Code!)
                .NotEmpty().WithMessage("Code cannot be empty if provided.");
        });

        // If name is provided, check not empty
        When(x => x.Name is not null, () => {
            RuleFor(x => x.Name!)
                .NotEmpty().WithMessage("Name cannot be empty if provided.");
        });
    }
}