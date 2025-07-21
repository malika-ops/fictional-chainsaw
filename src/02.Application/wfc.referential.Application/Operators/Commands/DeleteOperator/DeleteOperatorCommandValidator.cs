using FluentValidation;

namespace wfc.referential.Application.Operators.Commands.DeleteOperator;

public class DeleteOperatorCommandValidator : AbstractValidator<DeleteOperatorCommand>
{
    public DeleteOperatorCommandValidator()
    {
        RuleFor(x => x.OperatorId)
            .NotEqual(Guid.Empty).WithMessage("OperatorId must be a non-empty GUID.");
    }
}