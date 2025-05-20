using FluentValidation;

namespace wfc.referential.Application.TypeDefinitions.Commands.PatchTypeDefinition;

public class PatchTypeDefinitionCommandValidator : AbstractValidator<PatchTypeDefinitionCommand>
{
    public PatchTypeDefinitionCommandValidator()
    {
        RuleFor(x => x.TypeDefinitionId)
            .NotEqual(Guid.Empty).WithMessage("TypeDefinitionId cannot be empty");

        // Si Libelle est fourni, vérifier qu'il n'est pas vide
        When(x => x.Libelle is not null, () => {
            RuleFor(x => x.Libelle!)
            .NotEmpty().WithMessage("Libelle cannot be empty if provided");
        });

        // Si Description est fourni
        When(x => x.Description is not null, () => {
            RuleFor(x => x.Description!)
            .NotEmpty().WithMessage("Description cannot be empty if provided");
        });
    }
}