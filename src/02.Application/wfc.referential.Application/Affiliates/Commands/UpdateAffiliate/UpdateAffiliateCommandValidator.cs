using FluentValidation;

namespace wfc.referential.Application.Affiliates.Commands.UpdateAffiliate;

public class UpdateAffiliateCommandValidator : AbstractValidator<UpdateAffiliateCommand>
{
    public UpdateAffiliateCommandValidator()
    {
        RuleFor(x => x.AffiliateId)
            .NotEqual(Guid.Empty).WithMessage("AffiliateId cannot be empty");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required")
            .MaximumLength(50).WithMessage("Code cannot exceed 50 characters");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(255).WithMessage("Name cannot exceed 255 characters");

        RuleFor(x => x.Abbreviation)
            .MaximumLength(10).WithMessage("Abbreviation cannot exceed 10 characters");

        RuleFor(x => x.OpeningDate)
            .NotEmpty().WithMessage("OpeningDate is required");

        RuleFor(x => x.CountryId)
            .NotEqual(Guid.Empty).WithMessage("CountryId must be a valid GUID");

        RuleFor(x => x.AffiliateTypeId)
            .NotEqual(Guid.Empty).WithMessage("AffiliateTypeId is required and must be a valid GUID");

        RuleFor(x => x.ThresholdBilling)
            .GreaterThanOrEqualTo(0).WithMessage("ThresholdBilling must be greater than or equal to 0");

        RuleFor(x => x.AccountingDocumentNumber)
            .MaximumLength(100).WithMessage("AccountingDocumentNumber cannot exceed 100 characters");

        RuleFor(x => x.AccountingAccountNumber)
            .NotEmpty().WithMessage("AccountingAccountNumber is required")
            .MaximumLength(100).WithMessage("AccountingAccountNumber cannot exceed 100 characters");

        RuleFor(x => x.StampDutyMention)
            .MaximumLength(255).WithMessage("StampDutyMention cannot exceed 255 characters");

        RuleFor(x => x.Logo)
            .MaximumLength(500).WithMessage("Logo cannot exceed 500 characters");

        RuleFor(x => x.CancellationDay)
            .MaximumLength(50).WithMessage("CancellationDay cannot exceed 50 characters");
    }
}