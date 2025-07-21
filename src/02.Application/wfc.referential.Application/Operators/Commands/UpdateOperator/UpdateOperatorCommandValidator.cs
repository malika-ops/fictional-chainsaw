using FluentValidation;
using wfc.referential.Domain.OperatorAggregate;

namespace wfc.referential.Application.Operators.Commands.UpdateOperator;

public class UpdateOperatorCommandValidator : AbstractValidator<UpdateOperatorCommand>
{
    public UpdateOperatorCommandValidator()
    {
        RuleFor(x => x.OperatorId)
            .NotEqual(Guid.Empty).WithMessage("OperatorId cannot be empty");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required")
            .MaximumLength(50).WithMessage("Code cannot exceed 50 characters");

        RuleFor(x => x.IdentityCode)
            .NotEmpty().WithMessage("IdentityCode is required")
            .MaximumLength(50).WithMessage("IdentityCode cannot exceed 50 characters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("LastName is required")
            .MaximumLength(100).WithMessage("LastName cannot exceed 100 characters");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("FirstName is required")
            .MaximumLength(100).WithMessage("FirstName cannot exceed 100 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email must be a valid email address")
            .MaximumLength(255).WithMessage("Email cannot exceed 255 characters");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("PhoneNumber is required")
            .MaximumLength(20).WithMessage("PhoneNumber cannot exceed 20 characters");

        RuleFor(x => x.OperatorType)
            .IsInEnum().WithMessage("OperatorType must be a valid enum value")
            .When(x => x.OperatorType.HasValue);

        RuleFor(x => x.BranchId)
            .NotEqual(Guid.Empty).WithMessage("BranchId must be a valid GUID if provided")
            .When(x => x.BranchId.HasValue);
    }
}