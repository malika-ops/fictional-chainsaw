using FluentValidation;

namespace wfc.referential.Application.IdentityDocuments.Commands.CreateIdentityDocument;

public class CreateIdentityDocumentValidator : AbstractValidator<CreateIdentityDocumentCommand>
{
    public CreateIdentityDocumentValidator()
    {
        RuleFor(x => x.Code).NotEmpty()
            .WithMessage("Identity document code is required.");

        RuleFor(x => x.Name).NotEmpty()
            .WithMessage("Identity document name is required.");
    }
}