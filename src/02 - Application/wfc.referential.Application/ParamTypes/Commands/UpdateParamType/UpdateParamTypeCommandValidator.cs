using FluentValidation;

namespace wfc.referential.Application.ParamTypes.Commands.UpdateParamType;

public class UpdateParamTypeCommandValidator : AbstractValidator<UpdateParamTypeCommand>
{
    public UpdateParamTypeCommandValidator()
    {
        RuleFor(x => x.ParamTypeId)
            .NotNull().WithMessage("ParamTypeId cannot be null");

        RuleFor(x => x.TypeDefinitionId)
            .NotNull().WithMessage("TypeDefinitionId cannot be null");

        RuleFor(x => x.Value)
            .NotEmpty().WithMessage("Value is required");
    }
}