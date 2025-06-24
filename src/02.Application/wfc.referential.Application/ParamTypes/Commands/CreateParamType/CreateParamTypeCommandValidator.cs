using FluentValidation;

namespace wfc.referential.Application.ParamTypes.Commands.CreateParamType;

public class CreateParamTypeCommandValidator : AbstractValidator<CreateParamTypeCommand>
{
    public CreateParamTypeCommandValidator()
    {
        RuleFor(x => x.Value)
            .NotEmpty().WithMessage("Value is required.");

        RuleFor(x => x.TypeDefinitionId)
            .NotEqual(Guid.Empty).WithMessage("TypeDefinitionId is required.");
    }
}