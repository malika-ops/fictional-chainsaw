using FluentValidation;

namespace wfc.referential.Application.Partners.Commands.CreatePartner;

public class CreatePartnerCommandValidator : AbstractValidator<CreatePartnerCommand>
{
    public CreatePartnerCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required");

        RuleFor(x => x.Label)
            .NotEmpty().WithMessage("Label is required");

        RuleFor(x => x.IdPartner)
            .NotEmpty().WithMessage("Partner ID is required");

        RuleFor(x => x.SectorId)
            .NotEmpty().WithMessage("Sector ID is required");

        RuleFor(x => x.CityId)
            .NotEmpty().WithMessage("City ID is required");
    }
}