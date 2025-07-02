using FluentValidation;

namespace wfc.referential.Application.ContractDetails.Commands.CreateContractDetails;

public class CreateContractDetailsCommandValidator : AbstractValidator<CreateContractDetailsCommand>
{
    public CreateContractDetailsCommandValidator()
    {
        RuleFor(x => x.ContractId)
            .NotEqual(Guid.Empty).WithMessage("ContractId is required");

        RuleFor(x => x.PricingId)
            .NotEqual(Guid.Empty).WithMessage("PricingId is required");
    }
}