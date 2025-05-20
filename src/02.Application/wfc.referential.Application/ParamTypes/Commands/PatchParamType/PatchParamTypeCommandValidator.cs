using FluentValidation;

namespace wfc.referential.Application.ParamTypes.Commands.PatchParamType;

public class PatchParamTypeCommandValidator : AbstractValidator<PatchParamTypeCommand>
{
    public PatchParamTypeCommandValidator()
    {
        RuleFor(x => x.ParamTypeId)
            .NotEqual(Guid.Empty).WithMessage("ParamTypeId cannot be empty");

        // Si le champ Value est fourni, vérifier qu'il n'est pas vide
        When(x => x.Value is not null, () => {
            RuleFor(x => x.Value!)
            .NotEmpty().WithMessage("Value cannot be empty if provided");
        });
    }
}