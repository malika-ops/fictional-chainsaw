using FluentValidation;

namespace wfc.referential.Application.Sectors.Commands.DeleteSector;

public class DeleteSectorCommandValidator : AbstractValidator<DeleteSectorCommand>
{
    public DeleteSectorCommandValidator()
    {
        // Ensure the ID is non-empty & parseable as a GUID
        RuleFor(x => x.SectorId)
            .NotEmpty()
            .WithMessage("SectorId must be a non-empty GUID.");
    }
}