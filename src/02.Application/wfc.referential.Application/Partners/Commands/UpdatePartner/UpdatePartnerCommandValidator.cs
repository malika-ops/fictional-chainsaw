using FluentValidation;
using wfc.referential.Application.Partners.Commands.UpdatePartner;

public class UpdatePartnerCommandValidator : AbstractValidator<UpdatePartnerCommand>
{
    public UpdatePartnerCommandValidator()
    {
        RuleFor(x => x.PartnerId)
            .NotEqual(Guid.Empty).WithMessage("PartnerId cannot be empty");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required");

        RuleFor(x => x.Label)
            .NotEmpty().WithMessage("Label is required");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Type is required");
    }
}