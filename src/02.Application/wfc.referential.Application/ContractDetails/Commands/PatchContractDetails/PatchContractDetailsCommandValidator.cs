using FluentValidation;

namespace wfc.referential.Application.ContractDetails.Commands.PatchContractDetails;

public class PatchContractDetailsCommandValidator : AbstractValidator<PatchContractDetailsCommand>
{
    public PatchContractDetailsCommandValidator()
    {
        RuleFor(x => x.ContractDetailsId)
            .NotEqual(Guid.Empty).WithMessage("ContractDetailsId cannot be empty");

        // If ContractId is provided, check not empty
        When(x => x.ContractId.HasValue, () => {
            RuleFor(x => x.ContractId!.Value)
                .NotEqual(Guid.Empty).WithMessage("ContractId cannot be empty if provided");
        });

        // If PricingId is provided, check not empty
        When(x => x.PricingId.HasValue, () => {
            RuleFor(x => x.PricingId!.Value)
                .NotEqual(Guid.Empty).WithMessage("PricingId cannot be empty if provided");
        });
    }
}