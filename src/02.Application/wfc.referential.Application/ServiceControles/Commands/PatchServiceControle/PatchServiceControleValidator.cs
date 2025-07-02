using FluentValidation;

namespace wfc.referential.Application.ServiceControles.Commands.PatchServiceControle;

public class PatchServiceControleValidator : AbstractValidator<PatchServiceControleCommand>
{
    public PatchServiceControleValidator()
    {
        RuleFor(x => x.ServiceControleId)
            .NotEqual(Guid.Empty)
            .WithMessage("ServiceControleId cannot be empty.");

        When(x => x.ExecOrder.HasValue, () =>
            RuleFor(x => x.ExecOrder!.Value)
                .GreaterThanOrEqualTo(0)
                .WithMessage("ExecOrder must be ≥ 0."));
    }
}