using FluentValidation;

namespace wfc.referential.Application.Controles.Commands.CreateControle;

public class CreateControleValidator : AbstractValidator<CreateControleCommand>
{
    public CreateControleValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required.")
            .MaximumLength(50).WithMessage("Code max length = 50.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name max length = 100.");
    }
}
