using FluentValidation;

namespace wfc.referential.Application.Regions.Commands.PatchRegion;

public class PatchRegionValidator : AbstractValidator<PatchRegionCommand>
{

    public PatchRegionValidator()
    {

        When(x => x.Code is not null, () => {
            RuleFor(x => x.Code!)
                .NotEmpty()
                .WithMessage("Code cannot be empty if provided.");
        });

        When(x => x.Name is not null, () => {
            RuleFor(x => x.Name!)
            .NotEmpty()
            .WithMessage("Name cannot be empty if provided.");
        });

        When(x => x.IsEnabled is not null, () => {
            RuleFor(x => x.IsEnabled!)
            .NotEmpty()
            .WithMessage("IsEnabled cannot be empty if provided.");
        });

        When(x => x.CountryId is not null, () => {
            RuleFor(x => x.CountryId!)
            .NotEmpty()
            .WithMessage("Country Id cannot be empty if provided.");
        });
    }
}
