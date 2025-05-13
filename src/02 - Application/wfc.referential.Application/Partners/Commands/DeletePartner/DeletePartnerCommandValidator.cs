using FluentValidation;

namespace wfc.referential.Application.Partners.Commands.DeletePartner;

public class DeletePartnerCommandValidator : AbstractValidator<DeletePartnerCommand>
{
    public DeletePartnerCommandValidator()
    {
        // Ensure the ID is non-empty & parseable as a GUID
        RuleFor(x => x.PartnerId)
            .NotEmpty()
            .WithMessage("PartnerId must be a non-empty GUID.");
    }
}