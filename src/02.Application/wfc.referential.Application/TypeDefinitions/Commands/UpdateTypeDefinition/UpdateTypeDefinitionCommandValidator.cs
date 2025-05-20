using FluentValidation;

namespace wfc.referential.Application.TypeDefinitions.Commands.UpdateTypeDefinition;

public class UpdateTypeDefinitionCommandValidator : AbstractValidator<UpdateTypeDefinitionCommand>
{
    public UpdateTypeDefinitionCommandValidator()
    {
        // 1) Ensure ID is not empty
        RuleFor(x => x.TypeDefinitionId)
            .NotEqual(Guid.Empty).WithMessage("TypeDefinitionId cannot be empty.");

        // 3) Libelle not empty
        RuleFor(x => x.Libelle)
            .NotEmpty().WithMessage("Libelle is required");

        // 4) Description not empty
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required");
    }
}