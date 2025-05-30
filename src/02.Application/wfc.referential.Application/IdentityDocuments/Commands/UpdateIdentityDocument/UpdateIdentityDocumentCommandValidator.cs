using FluentValidation;

namespace wfc.referential.Application.IdentityDocuments.Commands.UpdateIdentityDocument;

public class UpdateIdentityDocumentCommandValidator : AbstractValidator<UpdateIdentityDocumentCommand>
{
    public UpdateIdentityDocumentCommandValidator()
    {
        RuleFor(x => x.IdentityDocumentId)
            .NotEqual(Guid.Empty)
            .WithMessage("IdentityDocumentId cannot be empty.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.");
    }
}