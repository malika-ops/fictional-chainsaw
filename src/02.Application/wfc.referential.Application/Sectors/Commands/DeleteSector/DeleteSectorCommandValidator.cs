using FluentValidation;

namespace wfc.referential.Application.Sectors.Commands.DeleteSector;

public class DeleteSectorCommandValidator : AbstractValidator<DeleteSectorCommand>
{
    public DeleteSectorCommandValidator()
    {
        RuleFor(x => x.SectorId)
            .NotEqual(Guid.Empty)
            .WithMessage("SectorId must be a non-empty GUID.");
    }
}