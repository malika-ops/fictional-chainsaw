using FluentValidation;


namespace wfc.referential.Application.TypeDefinitions.Commands.CreateTypeDefinition;

public class CreateTypeDefinitionCommandValidator : AbstractValidator<CreateTypeDefinitionCommand>
{

    public CreateTypeDefinitionCommandValidator()
    {
        RuleFor(x => x.Libelle)
            .NotEmpty().WithMessage("Name is required");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required");
    }

}

