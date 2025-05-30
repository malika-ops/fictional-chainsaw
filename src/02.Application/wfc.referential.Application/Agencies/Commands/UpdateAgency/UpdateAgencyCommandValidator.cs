using FluentValidation;

namespace wfc.referential.Application.Agencies.Commands.UpdateAgency;

public class UpdateAgencyCommandValidator : AbstractValidator<UpdateAgencyCommand>
{
    public UpdateAgencyCommandValidator()
    {
        RuleFor(x => x.AgencyId)
           .NotEqual(Guid.Empty)
           .WithMessage("AgencyId cannot be empty.");

        RuleFor(x => x.Code).NotEmpty();
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Abbreviation).NotEmpty();
        RuleFor(x => x.Address1).NotEmpty();
        RuleFor(x => x.Phone).NotEmpty();
        RuleFor(x => x.AccountingSheetName).NotEmpty();
        RuleFor(x => x.AccountingAccountNumber).NotEmpty();
        RuleFor(x => x.PostalCode).NotEmpty();

        RuleFor(x => x.Code)
            .Must(code => code.Length == 6 )
            .WithMessage("Code must be exactly 6 digits.");

        RuleFor(x => new { x.CityId, x.SectorId })
            .Must(v => v.CityId.HasValue ^ v.SectorId.HasValue)
            .WithMessage("Exactly one of CityId or SectorId must be supplied.");
    }
}