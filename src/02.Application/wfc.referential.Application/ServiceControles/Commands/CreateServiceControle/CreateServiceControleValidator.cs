using FluentValidation;

namespace wfc.referential.Application.ServiceControles.Commands.CreateServiceControle;

public class CreateServiceControleValidator : AbstractValidator<CreateServiceControleCommand>
{
    public CreateServiceControleValidator()
    {
        RuleFor(x => x.ServiceId)
            .NotEqual(Guid.Empty)
            .WithMessage("ServiceId must not be empty.");
        RuleFor(x => x.ControleId)
            .NotEqual(Guid.Empty)
            .WithMessage("ControleId must not be empty.");
        RuleFor(x => x.ChannelId)
            .NotEqual(Guid.Empty)
            .WithMessage("ChannelId must not be empty.");
        RuleFor(x => x.ExecOrder)
            .GreaterThanOrEqualTo(0)
            .WithMessage("ExecOrder must be greater than or equal to 0.");
    }
}