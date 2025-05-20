using FluentValidation;

namespace wfc.referential.Application.AgencyTiers.Commands.UpdateAgencyTier;

public class UpdateAgencyTierValidator : AbstractValidator<UpdateAgencyTierCommand>
{
    public UpdateAgencyTierValidator()
    {
        RuleFor(x => x.AgencyTierId)
            .NotEqual(Guid.Empty).WithMessage("AgencyTierId cannot be empty.");

        RuleFor(x => x.AgencyId)
            .NotEqual(Guid.Empty).WithMessage("AgencyId cannot be empty.");

        RuleFor(x => x.TierId)
            .NotEqual(Guid.Empty).WithMessage("TierId cannot be empty.");

        RuleFor(x => x.Code).NotEmpty().WithMessage("Code is required.");
        RuleFor(x => x.Password).NotEmpty().WithMessage("Password is required.");
    }
}