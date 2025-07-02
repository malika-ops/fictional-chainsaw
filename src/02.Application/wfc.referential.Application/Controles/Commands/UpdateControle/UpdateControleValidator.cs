using FluentValidation;

namespace wfc.referential.Application.Controles.Commands.UpdateControle;

public class UpdateControleValidator : AbstractValidator<UpdateControleCommand>
{
    public UpdateControleValidator()
    {
        RuleFor(x => x.ControleId)
            .NotEqual(Guid.Empty).WithMessage("ControleId cannot be empty.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required.")
            .MaximumLength(50).WithMessage("Code max length = 50.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name max length = 100.");
    }
}