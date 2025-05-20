using FluentValidation;

namespace wfc.referential.Application.ParamTypes.Commands.DeleteParamType;

public class DeleteParamTypeCommandValidator : AbstractValidator<DeleteParamTypeCommand>
{
    public DeleteParamTypeCommandValidator()
    {
        RuleFor(x => x.ParamTypeId)
            .NotEmpty().WithMessage("ParamTypeId is required.");
    }
}
