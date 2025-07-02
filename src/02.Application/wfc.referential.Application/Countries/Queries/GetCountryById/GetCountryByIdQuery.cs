using BuildingBlocks.Core.Abstraction.CQRS;
using wfc.referential.Application.Countries.Dtos;

namespace wfc.referential.Application.Countries.Queries.GetCountryById;

public record GetCountryByIdQuery : IQuery<GetCountriesResponce>
{
    public Guid CountryId { get; init; }
} 