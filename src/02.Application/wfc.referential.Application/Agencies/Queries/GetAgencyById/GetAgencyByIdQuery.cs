using BuildingBlocks.Core.Abstraction.CQRS;
using wfc.referential.Application.Agencies.Dtos;

namespace wfc.referential.Application.Agencies.Queries.GetAgencyById;

public record GetAgencyByIdQuery : IQuery<GetAgenciesResponse>
{
    public Guid AgencyId { get; init; }
} 