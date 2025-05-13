using FluentValidation;

namespace wfc.referential.Application.Corridors.Commands.UpdateCorridor;

public class UpdateCorridorValidator : AbstractValidator<UpdateCorridorCommand>
{
    public UpdateCorridorValidator()
    {
        RuleFor(c => c.CorridorId)
            .NotEqual(Guid.Empty).WithMessage(c => $"{c.CorridorId} cannot be empty.");
    }
}
