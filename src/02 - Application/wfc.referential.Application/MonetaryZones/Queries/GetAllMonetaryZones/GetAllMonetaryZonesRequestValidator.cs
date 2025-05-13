using FluentValidation;

namespace wfc.referential.Application.MonetaryZones.Queries.GetAllMonetaryZones;

public class GetAllMonetaryZonesRequestValidator : AbstractValidator<GetAllMonetaryZonesQuery>
{
    public GetAllMonetaryZonesRequestValidator()
    {
    }
}
