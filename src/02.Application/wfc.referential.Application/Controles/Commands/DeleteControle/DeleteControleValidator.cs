using FluentValidation;

namespace wfc.referential.Application.Controles.Commands.DeleteControle;

public class DeleteControleValidator : AbstractValidator<DeleteControleCommand>
{
    public DeleteControleValidator()
    {
        RuleFor(x => x.ControleId)
            .NotEqual(Guid.Empty)
            .WithMessage("ControleId must be a non-empty GUID.");
    }
}