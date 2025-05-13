using FluentValidation;

namespace wfc.referential.Application.Cities.Queries.GetAllCities;

public class GetAllCitiesRequestValidator : AbstractValidator<GetAllCitiesQuery>
{
    public GetAllCitiesRequestValidator()
    {
    }
}
