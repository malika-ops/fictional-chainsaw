using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wfc.referential.Application.Pricings.Commands.DeletePricing;

public class DeletePricingValidator : AbstractValidator<DeletePricingCommand>
{
    public DeletePricingValidator()
    {
        RuleFor(x => x.PricingId)
            .NotEqual(Guid.Empty)
            .WithMessage("PricingId must be a non-empty GUID.");
    }
}