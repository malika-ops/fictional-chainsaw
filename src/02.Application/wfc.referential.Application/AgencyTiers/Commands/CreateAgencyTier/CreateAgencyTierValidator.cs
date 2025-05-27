using FluentValidation;

namespace wfc.referential.Application.AgencyTiers.Commands.CreateAgencyTier;

public class CreateAgencyTierValidator : AbstractValidator<CreateAgencyTierCommand>
{
    public CreateAgencyTierValidator()
    {
        RuleFor(x => x.AgencyId)
            .NotEqual(Guid.Empty).WithMessage("AgencyId cannot be empty.");


        RuleFor(x => x.TierId)
            .NotEqual(Guid.Empty).WithMessage("TierId cannot be empty.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required.")
            .MaximumLength(30).WithMessage("Code max length = 30.");
    }
}