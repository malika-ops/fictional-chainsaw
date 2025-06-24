using FluentValidation;

namespace wfc.referential.Application.ParamTypes.Commands.UpdateParamType;

public class UpdateParamTypeCommandValidator : AbstractValidator<UpdateParamTypeCommand>
{
    public UpdateParamTypeCommandValidator()
    {
        RuleFor(x => x.ParamTypeId)
            .NotEqual(Guid.Empty)
            .WithMessage("ParamTypeId cannot be empty.");

        RuleFor(x => x.Value)
            .NotEmpty().WithMessage("Value is required.");

        RuleFor(x => x.TypeDefinitionId)
            .NotEqual(Guid.Empty)
            .WithMessage("TypeDefinitionId cannot be empty.");
    }
}