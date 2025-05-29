using FluentValidation;

namespace wfc.referential.Application.Partners.Commands.UpdatePartner;

public class UpdatePartnerCommandValidator : AbstractValidator<UpdatePartnerCommand>
{
    public UpdatePartnerCommandValidator()
    {
        RuleFor(x => x.PartnerId)
            .NotEqual(Guid.Empty).WithMessage("PartnerId cannot be empty");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required");

        RuleFor(x => x.PersonType)
            .NotEmpty().WithMessage("PersonType is required");
    }
}