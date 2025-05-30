using FluentValidation;

namespace wfc.referential.Application.Agencies.Commands.CreateAgency;

public class CreateAgencyValidator : AbstractValidator<CreateAgencyCommand>
{
    public CreateAgencyValidator()
    {
        RuleFor(x => x.Name).NotEmpty()
            .WithMessage("Agency Name is required.");
        RuleFor(x => x.Abbreviation).NotEmpty()
            .WithMessage("Abbreviation is required.");
        RuleFor(x => x.Address1).NotEmpty()
            .WithMessage("Address is required.");
        RuleFor(x => x.Phone).NotEmpty()
            .WithMessage("Phone number is required.");
        RuleFor(x => x.AccountingSheetName).NotEmpty()
            .WithMessage("Accounting sheet name is required.");
        RuleFor(x => x.AccountingAccountNumber).NotEmpty()
            .WithMessage("Accounting account number is required.");
        RuleFor(x => x.PostalCode).NotEmpty()
            .WithMessage("Postal code is required.");

        When(x => !string.IsNullOrWhiteSpace(x.Code), () =>
        {
            RuleFor(x => x.Code!)
                .Length(6)
                .WithMessage("Agency code must be exactly 6 digits when provided.");
        });

        /* Exactly one of CityId / SectorId must be filled */
        RuleFor(x => new { x.CityId, x.SectorId })
            .Must(link => link.CityId.HasValue ^ link.SectorId.HasValue)
            .WithMessage("Either CityId or SectorId must be provided (but not both).");

    }
}