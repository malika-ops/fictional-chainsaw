using FluentValidation;

namespace wfc.referential.Application.Agencies.Commands.PatchAgency;

public class PatchAgencyCommandValidator : AbstractValidator<PatchAgencyCommand>
{
    public PatchAgencyCommandValidator()
    {
        RuleFor(x => x.AgencyId)
            .NotEqual(Guid.Empty).WithMessage("AgencyId cannot be empty.");

        // exactly one of City / Sector if any of them supplied
        RuleFor(x => new { x.CityId, x.SectorId })
          .Must(v => !(v.CityId.HasValue && v.SectorId.HasValue))
          .WithMessage("CityId and SectorId are mutually exclusive.");

        RuleFor(x => new { x.CityId, x.SectorId })
          .Must(v => (v.CityId.HasValue || v.SectorId.HasValue) || // none supplied is fine (no change)
                     !(v.CityId.HasValue ^ v.SectorId.HasValue))    // but not both
          .WithMessage("Provide at most one of CityId or SectorId.");
    }
}