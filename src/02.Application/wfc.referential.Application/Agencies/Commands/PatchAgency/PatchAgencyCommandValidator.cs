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

        When(x => !string.IsNullOrWhiteSpace(x.Code), () =>
        {
            RuleFor(x => x.Code!)
                .Length(6)
                .WithMessage("Agency code must be exactly 6 digits when provided.");
        });

    }
}