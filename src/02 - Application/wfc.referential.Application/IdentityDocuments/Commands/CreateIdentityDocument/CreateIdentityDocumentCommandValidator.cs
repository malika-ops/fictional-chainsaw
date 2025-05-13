using FluentValidation;

namespace wfc.referential.Application.IdentityDocuments.Commands.CreateIdentityDocument;

public class CreateIdentityDocumentCommandValidator : AbstractValidator<CreateIdentityDocumentCommand>
{
    public CreateIdentityDocumentCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty().WithMessage("Code is required");
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required");
    }
}