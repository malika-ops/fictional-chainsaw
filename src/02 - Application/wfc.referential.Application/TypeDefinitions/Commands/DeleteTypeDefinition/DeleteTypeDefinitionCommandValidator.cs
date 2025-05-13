using FluentValidation;

namespace wfc.referential.Application.TypeDefinitions.Commands.DeleteTypeDefinition;

public class DeleteTypeDefinitionCommandValidator : AbstractValidator<DeleteTypeDefinitionCommand>
{

    public DeleteTypeDefinitionCommandValidator()
    {

        // ensure the ID is non-empty & parseable as a GUID
        RuleFor(x => x.TypeDefinitionId)
            .NotEmpty()
            .WithMessage("TypeDefinitionId must be a non-empty GUID.");
    }
}
