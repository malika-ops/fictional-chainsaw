using FluentValidation;

namespace wfc.referential.Application.ContractDetails.Commands.UpdateContractDetails;

public class UpdateContractDetailsCommandValidator : AbstractValidator<UpdateContractDetailsCommand>
{
    public UpdateContractDetailsCommandValidator()
    {
        RuleFor(x => x.ContractDetailsId)
            .NotEqual(Guid.Empty).WithMessage("ContractDetailsId cannot be empty");

        RuleFor(x => x.ContractId)
            .NotEqual(Guid.Empty).WithMessage("ContractId is required");

        RuleFor(x => x.PricingId)
            .NotEqual(Guid.Empty).WithMessage("PricingId is required");
    }
}