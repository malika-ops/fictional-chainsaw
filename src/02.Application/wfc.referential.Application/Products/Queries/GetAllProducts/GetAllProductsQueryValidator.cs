using FluentValidation;

namespace wfc.referential.Application.Products.Queries.GetAllProducts;

public class GetAllMonetaryZonesRequestValidator : AbstractValidator<GetAllProductsQuery>
{
    public GetAllMonetaryZonesRequestValidator()
    {
    }
}
