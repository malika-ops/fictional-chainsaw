using FluentValidation;

namespace wfc.referential.Application.Cities.Queries.GetFiltredCities;

public class GetFiltredCitiesRequestValidator : AbstractValidator<GetFiltredCitiesQuery>
{
    public GetFiltredCitiesRequestValidator()
    {
    }
}
