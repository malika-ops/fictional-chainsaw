﻿using FluentValidation;

namespace wfc.referential.Application.TaxRuleDetails.Queries.GetFiltredTaxeRuleDetails;

public class GetFiltredTaxRuleDetailsRequestValidator : AbstractValidator<GetFiltredTaxRuleDetailsQuery>
{
    public GetFiltredTaxRuleDetailsRequestValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("Page number must be greater than 0");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100");

    }
}