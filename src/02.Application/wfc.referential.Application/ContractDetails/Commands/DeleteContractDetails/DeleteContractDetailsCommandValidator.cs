using FluentValidation;

namespace wfc.referential.Application.ContractDetails.Commands.DeleteContractDetails;

public class DeleteContractDetailsCommandValidator : AbstractValidator<DeleteContractDetailsCommand>
{
    public DeleteContractDetailsCommandValidator()
    {
        RuleFor(x => x.ContractDetailsId)
            .NotEqual(Guid.Empty).WithMessage("ContractDetailsId must be a non-empty GUID.");
    }
}