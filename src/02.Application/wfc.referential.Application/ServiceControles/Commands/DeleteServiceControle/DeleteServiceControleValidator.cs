using FluentValidation;

namespace wfc.referential.Application.ServiceControles.Commands.DeleteServiceControle;

public class DeleteServiceControleValidator : AbstractValidator<DeleteServiceControleCommand>
{
    public DeleteServiceControleValidator()
    {
        RuleFor(x => x.ServiceControleId)
            .NotEqual(Guid.Empty)
            .WithMessage("ServiceControleId must be a non-empty GUID.");
    }
}