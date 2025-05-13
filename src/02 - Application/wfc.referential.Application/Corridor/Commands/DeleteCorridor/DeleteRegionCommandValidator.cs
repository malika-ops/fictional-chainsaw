using FluentValidation;


namespace wfc.referential.Application.Corridors.Commands.DeleteCorridor;

public class DeleteRegionCommandValidator : AbstractValidator<DeleteCorridorCommand>
{

    public DeleteRegionCommandValidator()
    {
        RuleFor(c => c.CorridorId)
            .NotEmpty().WithMessage(c => $"{c.CorridorId} is required.");
    }
}
