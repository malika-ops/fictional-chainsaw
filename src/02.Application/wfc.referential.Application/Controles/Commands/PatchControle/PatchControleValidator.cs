using FluentValidation;

namespace wfc.referential.Application.Controles.Commands.PatchControle;

public class PatchControleValidator : AbstractValidator<PatchControleCommand>
{
    public PatchControleValidator()
    {
        RuleFor(x => x.ControleId)
            .NotEqual(Guid.Empty)
            .WithMessage("ControleId cannot be empty.");

        When(x => x.Code is not null, () =>
            RuleFor(x => x.Code!)
                .NotEmpty().WithMessage("Code cannot be empty if provided.")
                .MaximumLength(50).WithMessage("Code max length = 50."));

        When(x => x.Name is not null, () =>
            RuleFor(x => x.Name!)
                .NotEmpty().WithMessage("Name cannot be empty if provided.")
                .MaximumLength(100).WithMessage("Name max length = 100."));
    }
}
