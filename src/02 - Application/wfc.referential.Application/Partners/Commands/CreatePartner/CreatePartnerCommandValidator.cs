using FluentValidation;
using wfc.referential.Application.Partners.Commands.CreatePartner;

public class CreatePartnerCommandValidator : AbstractValidator<CreatePartnerCommand>
{
    public CreatePartnerCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required");

        RuleFor(x => x.Label)
            .NotEmpty().WithMessage("Label is required");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Type is required");
    }
}