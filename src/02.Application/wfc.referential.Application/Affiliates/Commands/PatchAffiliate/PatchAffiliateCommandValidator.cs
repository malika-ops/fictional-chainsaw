using FluentValidation;

namespace wfc.referential.Application.Affiliates.Commands.PatchAffiliate;

public class PatchAffiliateCommandValidator : AbstractValidator<PatchAffiliateCommand>
{
    public PatchAffiliateCommandValidator()
    {
        RuleFor(x => x.AffiliateId)
            .NotEqual(Guid.Empty).WithMessage("AffiliateId cannot be empty");

        // If code is provided, check not empty and length
        When(x => x.Code is not null, () => {
            RuleFor(x => x.Code!)
                .NotEmpty().WithMessage("Code cannot be empty if provided")
                .MaximumLength(50).WithMessage("Code cannot exceed 50 characters");
        });

        // If name is provided, check not empty and length
        When(x => x.Name is not null, () => {
            RuleFor(x => x.Name!)
                .NotEmpty().WithMessage("Name cannot be empty if provided")
                .MaximumLength(255).WithMessage("Name cannot exceed 255 characters");
        });

        // If abbreviation is provided, check length
        When(x => x.Abbreviation is not null, () => {
            RuleFor(x => x.Abbreviation!)
                .MaximumLength(10).WithMessage("Abbreviation cannot exceed 10 characters");
        });

        // If CountryId is provided, check valid GUID
        When(x => x.CountryId is not null, () => {
            RuleFor(x => x.CountryId!.Value)
                .NotEqual(Guid.Empty).WithMessage("CountryId must be a valid GUID if provided");
        });

        // If ThresholdBilling is provided, check positive
        When(x => x.ThresholdBilling is not null, () => {
            RuleFor(x => x.ThresholdBilling!.Value)
                .GreaterThanOrEqualTo(0).WithMessage("ThresholdBilling must be greater than or equal to 0");
        });

        // Optional field length validations
        When(x => x.AccountingDocumentNumber is not null, () => {
            RuleFor(x => x.AccountingDocumentNumber!)
                .MaximumLength(100).WithMessage("AccountingDocumentNumber cannot exceed 100 characters");
        });

        When(x => x.AccountingAccountNumber is not null, () => {
            RuleFor(x => x.AccountingAccountNumber!)
                .MaximumLength(100).WithMessage("AccountingAccountNumber cannot exceed 100 characters");
        });

        When(x => x.StampDutyMention is not null, () => {
            RuleFor(x => x.StampDutyMention!)
                .MaximumLength(255).WithMessage("StampDutyMention cannot exceed 255 characters");
        });

        When(x => x.Logo is not null, () => {
            RuleFor(x => x.Logo!)
                .MaximumLength(500).WithMessage("Logo cannot exceed 500 characters");
        });

        When(x => x.AffiliateType is not null, () => {
            RuleFor(x => x.AffiliateType!)
                .IsInEnum().WithMessage("AffiliateType must be a valid enum value");
        });

        When(x => x.OpeningDate is not null, () => {
            RuleFor(x => x.OpeningDate!)
                .LessThanOrEqualTo(DateTime.Today).WithMessage("OpeningDate cannot be in the future");
        });

        When(x => x.CancellationDay is not null, () => {
            RuleFor(x => x.CancellationDay!)
                .MaximumLength(50).WithMessage("CancellationDay cannot exceed 50 characters");
        });
    }
}