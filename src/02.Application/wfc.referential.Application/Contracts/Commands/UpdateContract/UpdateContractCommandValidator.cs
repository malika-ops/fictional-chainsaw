using FluentValidation;

namespace wfc.referential.Application.Contracts.Commands.UpdateContract;

public class UpdateContractCommandValidator : AbstractValidator<UpdateContractCommand>
{
    public UpdateContractCommandValidator()
    {
        RuleFor(x => x.ContractId)
            .NotEqual(Guid.Empty).WithMessage("ContractId cannot be empty");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required");

        RuleFor(x => x.PartnerId)
            .NotEqual(Guid.Empty).WithMessage("PartnerId is required");

        RuleFor(x => x.StartDate)
            .NotEqual(default(DateTime)).WithMessage("StartDate is required");

        RuleFor(x => x.EndDate)
            .NotEqual(default(DateTime)).WithMessage("EndDate is required");

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate).WithMessage("EndDate must be greater than StartDate");
    }
}