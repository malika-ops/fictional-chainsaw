using FluentValidation;

namespace wfc.referential.Application.Taxes.Queries.GetAllTaxes;

public class GetAllTaxesRequestValidator : AbstractValidator<GetAllTaxesQuery>
{
    public GetAllTaxesRequestValidator()
    {
    }
}
