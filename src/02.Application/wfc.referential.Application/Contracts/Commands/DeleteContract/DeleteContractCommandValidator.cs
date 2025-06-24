using FluentValidation;

namespace wfc.referential.Application.Contracts.Commands.DeleteContract;

public class DeleteContractCommandValidator : AbstractValidator<DeleteContractCommand>
{
    public DeleteContractCommandValidator()
    {
        RuleFor(x => x.ContractId)
            .NotEqual(Guid.Empty).WithMessage("ContractId must be a non-empty GUID.");
    }
}