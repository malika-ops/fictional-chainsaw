using FluentValidation;

namespace wfc.referential.Application.ServiceControles.Commands.UpdateServiceControle;

public class UpdateServiceControleValidator : AbstractValidator<UpdateServiceControleCommand>
{
    public UpdateServiceControleValidator()
    {
        RuleFor(x => x.ServiceControleId)
            .NotEqual(Guid.Empty).WithMessage("ServiceControleId cannot be empty.");

        RuleFor(x => x.ServiceId)
            .NotEqual(Guid.Empty).WithMessage("ServiceId cannot be empty.");

        RuleFor(x => x.ControleId)
            .NotEqual(Guid.Empty).WithMessage("ControleId cannot be empty.");

        RuleFor(x => x.ChannelId)
            .NotEqual(Guid.Empty).WithMessage("ChannelId cannot be empty.");

        RuleFor(x => x.ExecOrder)
            .GreaterThanOrEqualTo(0).WithMessage("ExecOrder must be ≥ 0.");
    }
}