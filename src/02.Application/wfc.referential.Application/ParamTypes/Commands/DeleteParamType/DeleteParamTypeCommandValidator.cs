using FluentValidation;

namespace wfc.referential.Application.ParamTypes.Commands.DeleteParamType;

public class DeleteParamTypeCommandValidator : AbstractValidator<DeleteParamTypeCommand>
{
    public DeleteParamTypeCommandValidator()
    {
        RuleFor(x => x.ParamTypeId)
            .NotEqual(Guid.Empty)
            .WithMessage("ParamTypeId must be a non-empty GUID.");
    }
}