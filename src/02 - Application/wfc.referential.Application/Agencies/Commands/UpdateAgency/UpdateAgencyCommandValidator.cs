using FluentValidation;

namespace wfc.referential.Application.Agencies.Commands.UpdateAgency;

public class UpdateAgencyCommandValidator : AbstractValidator<UpdateAgencyCommand>
{
    public UpdateAgencyCommandValidator()
    {
        RuleFor(x => x.AgencyId)
            .NotEqual(Guid.Empty)
            .WithMessage("AgencyId cannot be empty.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.");

        RuleFor(x => x.Abbreviation)
            .NotEmpty().WithMessage("Abbreviation is required.");

        RuleFor(x => x.Address1)
            .NotEmpty().WithMessage("Address1 is required.");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone is required.");

        // Exclusive City/Sector check
        RuleFor(x => new { x.CityId, x.SectorId })
            .Must(v => (v.CityId.HasValue ^ v.SectorId.HasValue))
            .WithMessage("Exactly one of CityId or SectorId must be supplied.");
    }
}