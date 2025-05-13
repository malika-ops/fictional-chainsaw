using FluentValidation;

namespace wfc.referential.Application.Regions.Commands.UpdateRegion;

public class UpdateRegionValidator : AbstractValidator<UpdateRegionCommand>
{
    public UpdateRegionValidator()
    {
        RuleFor(x => x.RegionId)
            .NotEqual(Guid.Empty).WithMessage("RegionId cannot be empty.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required");

        RuleFor(x => x.IsEnabled)
            .NotEmpty().WithMessage("IsEnabled is required");

        RuleFor(x => x.CountryId)
            .NotEmpty().WithMessage("CountryID is required");
    }
}
