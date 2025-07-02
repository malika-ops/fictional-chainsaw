using BuildingBlocks.Core.Abstraction.CQRS;
using wfc.referential.Application.CountryServices.Dtos;

namespace wfc.referential.Application.CountryServices.Queries.GetCountryServiceById;

public record GetCountryServiceByIdQuery : IQuery<GetCountryServicesResponse>
{
    public Guid CountryServiceId { get; init; }
} 