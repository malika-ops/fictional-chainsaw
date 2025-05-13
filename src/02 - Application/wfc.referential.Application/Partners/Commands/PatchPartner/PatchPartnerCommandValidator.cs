using FluentValidation;

namespace wfc.referential.Application.Partners.Commands.PatchPartner;

public class PatchPartnerCommandValidator : AbstractValidator<PatchPartnerCommand>
{
    public PatchPartnerCommandValidator()
    {
        RuleFor(x => x.PartnerId)
            .NotEqual(Guid.Empty).WithMessage("PartnerId cannot be empty");

        // If code is provided, check not empty
        When(x => x.Code is not null, () => {
            RuleFor(x => x.Code!)
                .NotEmpty().WithMessage("Code cannot be empty if provided");
        });

        // If label is provided, check not empty
        When(x => x.Label is not null, () => {
            RuleFor(x => x.Label!)
                .NotEmpty().WithMessage("Label cannot be empty if provided");
        });

        // If IdPartner is provided, check not empty
        When(x => x.IdPartner is not null, () => {
            RuleFor(x => x.IdPartner!)
                .NotEmpty().WithMessage("IdPartner cannot be empty if provided");
        });

        // If SectorId is provided, check not empty
        When(x => x.SectorId.HasValue, () => {
            RuleFor(x => x.SectorId!.Value)
                .NotEqual(Guid.Empty).WithMessage("SectorId cannot be empty if provided");
        });

        // If CityId is provided, check not empty
        When(x => x.CityId.HasValue, () => {
            RuleFor(x => x.CityId!.Value)
                .NotEqual(Guid.Empty).WithMessage("CityId cannot be empty if provided");
        });
    }
}