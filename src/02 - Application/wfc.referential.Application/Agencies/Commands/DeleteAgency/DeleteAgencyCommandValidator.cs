using FluentValidation;

namespace wfc.referential.Application.Agencies.Commands.DeleteAgency;

public class DeleteAgencyCommandValidator : AbstractValidator<DeleteAgencyCommand>
{
    public DeleteAgencyCommandValidator()
    {
        RuleFor(x => x.AgencyId)
            .NotEqual(Guid.Empty)
            .WithMessage("AgencyId must be a non-empty GUID.");
    }
}