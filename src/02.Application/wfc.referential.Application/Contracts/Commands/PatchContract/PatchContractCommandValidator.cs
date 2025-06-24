using FluentValidation;

namespace wfc.referential.Application.Contracts.Commands.PatchContract;

public class PatchContractCommandValidator : AbstractValidator<PatchContractCommand>
{
    public PatchContractCommandValidator()
    {
        RuleFor(x => x.ContractId)
            .NotEqual(Guid.Empty).WithMessage("ContractId cannot be empty");

        // If code is provided, check not empty
        When(x => x.Code is not null, () => {
            RuleFor(x => x.Code!)
                .NotEmpty().WithMessage("Code cannot be empty if provided");
        });

        // If PartnerId is provided, check not empty
        When(x => x.PartnerId.HasValue, () => {
            RuleFor(x => x.PartnerId!.Value)
                .NotEqual(Guid.Empty).WithMessage("PartnerId cannot be empty if provided");
        });

        // If both StartDate and EndDate are provided, validate relationship
        When(x => x.StartDate.HasValue && x.EndDate.HasValue, () => {
            RuleFor(x => x.EndDate!.Value)
                .GreaterThan(x => x.StartDate!.Value).WithMessage("EndDate must be greater than StartDate");
        });
    }
}