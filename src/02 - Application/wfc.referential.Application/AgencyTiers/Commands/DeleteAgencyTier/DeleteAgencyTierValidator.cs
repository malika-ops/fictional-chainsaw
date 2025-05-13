using FluentValidation;

namespace wfc.referential.Application.AgencyTiers.Commands.DeleteAgencyTier;

public class DeleteAgencyTierValidator : AbstractValidator<DeleteAgencyTierCommand>
{
    public DeleteAgencyTierValidator()
    {
        RuleFor(x => x.AgencyTierId)
            .NotEqual(Guid.Empty)
            .WithMessage("AgencyTierId must be a non-empty GUID.");
    }
}