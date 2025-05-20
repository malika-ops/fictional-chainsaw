using FluentValidation;

namespace wfc.referential.Application.Sectors.Commands.UpdateSector;

public class UpdateSectorCommandValidator : AbstractValidator<UpdateSectorCommand>
{
    public UpdateSectorCommandValidator()
    {
        RuleFor(x => x.SectorId)
            .NotEqual(Guid.Empty).WithMessage("SectorId cannot be empty");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required")
            .MaximumLength(50).WithMessage("Code must not exceed 50 characters");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters");

        RuleFor(x => x.CityId)
            .NotEmpty().WithMessage("City ID is required");
    }
}