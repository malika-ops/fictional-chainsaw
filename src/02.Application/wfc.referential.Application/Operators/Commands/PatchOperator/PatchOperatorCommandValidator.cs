using FluentValidation;
using wfc.referential.Domain.OperatorAggregate;

namespace wfc.referential.Application.Operators.Commands.PatchOperator;

public class PatchOperatorCommandValidator : AbstractValidator<PatchOperatorCommand>
{
    public PatchOperatorCommandValidator()
    {
        RuleFor(x => x.OperatorId)
            .NotEqual(Guid.Empty).WithMessage("OperatorId cannot be empty");

        // If code is provided, check not empty and length
        When(x => x.Code is not null, () => {
            RuleFor(x => x.Code!)
                .NotEmpty().WithMessage("Code cannot be empty if provided")
                .MaximumLength(50).WithMessage("Code cannot exceed 50 characters");
        });

        // If identity code is provided, check not empty and length
        When(x => x.IdentityCode is not null, () => {
            RuleFor(x => x.IdentityCode!)
                .NotEmpty().WithMessage("IdentityCode cannot be empty if provided")
                .MaximumLength(50).WithMessage("IdentityCode cannot exceed 50 characters");
        });

        // If last name is provided, check not empty and length
        When(x => x.LastName is not null, () => {
            RuleFor(x => x.LastName!)
                .NotEmpty().WithMessage("LastName cannot be empty if provided")
                .MaximumLength(100).WithMessage("LastName cannot exceed 100 characters");
        });

        // If first name is provided, check not empty and length
        When(x => x.FirstName is not null, () => {
            RuleFor(x => x.FirstName!)
                .NotEmpty().WithMessage("FirstName cannot be empty if provided")
                .MaximumLength(100).WithMessage("FirstName cannot exceed 100 characters");
        });

        // If email is provided, check valid format and length
        When(x => x.Email is not null, () => {
            RuleFor(x => x.Email!)
                .NotEmpty().WithMessage("Email cannot be empty if provided")
                .EmailAddress().WithMessage("Email must be a valid email address")
                .MaximumLength(255).WithMessage("Email cannot exceed 255 characters");
        });

        // If phone number is provided, check not empty and length
        When(x => x.PhoneNumber is not null, () => {
            RuleFor(x => x.PhoneNumber!)
                .NotEmpty().WithMessage("PhoneNumber cannot be empty if provided")
                .MaximumLength(20).WithMessage("PhoneNumber cannot exceed 20 characters");
        });

        // If operator type is provided, check valid enum
        When(x => x.OperatorType is not null, () => {
            RuleFor(x => x.OperatorType!.Value)
                .IsInEnum().WithMessage("OperatorType must be a valid enum value");
        });

        // If branch ID is provided, check valid GUID
        When(x => x.BranchId is not null, () => {
            RuleFor(x => x.BranchId!.Value)
                .NotEqual(Guid.Empty).WithMessage("BranchId must be a valid GUID if provided");
        });
    }
}
