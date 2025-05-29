using FluentValidation;

namespace wfc.referential.Application.Partners.Commands.DeletePartner;

public class DeletePartnerCommandValidator : AbstractValidator<DeletePartnerCommand>
{
    public DeletePartnerCommandValidator()
    {
        RuleFor(x => x.PartnerId)
            .NotEqual(Guid.Empty).WithMessage("PartnerId must be a non-empty GUID.");
    }
}