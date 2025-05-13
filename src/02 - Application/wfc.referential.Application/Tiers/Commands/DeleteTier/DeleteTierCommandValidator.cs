using FluentValidation;

namespace wfc.referential.Application.Tiers.Commands.DeleteTier;

public class DeleteTierCommandValidator : AbstractValidator<DeleteTierCommand>
{
    public DeleteTierCommandValidator()
    {
        RuleFor(x => x.TierId)
            .NotEqual(Guid.Empty)
            .WithMessage("TierId must be a non-empty GUID.");
    }
}