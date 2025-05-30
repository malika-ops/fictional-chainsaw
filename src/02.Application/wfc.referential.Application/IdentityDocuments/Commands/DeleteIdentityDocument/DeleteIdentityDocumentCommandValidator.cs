using FluentValidation;

namespace wfc.referential.Application.IdentityDocuments.Commands.DeleteIdentityDocument;

public class DeleteIdentityDocumentCommandValidator : AbstractValidator<DeleteIdentityDocumentCommand>
{
    public DeleteIdentityDocumentCommandValidator()
    {
        RuleFor(x => x.IdentityDocumentId)
            .NotEqual(Guid.Empty)
            .WithMessage("IdentityDocumentId must be a non-empty GUID.");
    }
}