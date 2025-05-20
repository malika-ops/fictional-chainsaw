using FluentValidation;

namespace wfc.referential.Application.Sectors.Commands.PatchSector;

public class PatchSectorCommandValidator : AbstractValidator<PatchSectorCommand>
{
    public PatchSectorCommandValidator()
    {
        RuleFor(x => x.SectorId)
            .NotEqual(Guid.Empty).WithMessage("SectorId cannot be empty");

        // If code is provided (not null), check it's not empty
        When(x => x.Code is not null, () => {
            RuleFor(x => x.Code!)
                .NotEmpty().WithMessage("Code cannot be empty if provided")
                .MaximumLength(50).WithMessage("Code must not exceed 50 characters");
        });

        // If name is provided, check not empty
        When(x => x.Name is not null, () => {
            RuleFor(x => x.Name!)
                .NotEmpty().WithMessage("Name cannot be empty if provided")
                .MaximumLength(100).WithMessage("Name must not exceed 100 characters");
        });

        // If CityId is provided, check not empty
        When(x => x.CityId.HasValue, () => {
            RuleFor(x => x.CityId!.Value)
                .NotEqual(Guid.Empty).WithMessage("CityId cannot be empty if provided");
        });
    }
}