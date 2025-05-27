using FluentValidation;

namespace wfc.referential.Application.Sectors.Commands.UpdateSector;

public class UpdateSectorCommandValidator : AbstractValidator<UpdateSectorCommand>
{
    public UpdateSectorCommandValidator()
    {
        RuleFor(x => x.SectorId)
            .NotEqual(Guid.Empty)
            .WithMessage("SectorId cannot be empty.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.");

        RuleFor(x => x.CityId)
            .NotEqual(Guid.Empty)
            .WithMessage("CityId is required.");
    }
}
