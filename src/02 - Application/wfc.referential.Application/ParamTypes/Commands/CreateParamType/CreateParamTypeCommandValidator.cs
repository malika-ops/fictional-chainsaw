using FluentValidation;

namespace wfc.referential.Application.ParamTypes.Commands.CreateParamType;

public class CreateParamTypeCommandValidator : AbstractValidator<CreateParamTypeCommand>
{
    public CreateParamTypeCommandValidator()
    {

        RuleFor(x => x.TypeDefinitionId)
            .NotEmpty().WithMessage("Type Definition is required");

        RuleFor(x => x.Value)
            .NotEmpty().WithMessage("Value is required");

    }
}