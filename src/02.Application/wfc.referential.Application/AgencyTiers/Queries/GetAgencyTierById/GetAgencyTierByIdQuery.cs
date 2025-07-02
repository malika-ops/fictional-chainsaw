using BuildingBlocks.Core.Abstraction.CQRS;
using wfc.referential.Application.AgencyTiers.Dtos;

namespace wfc.referential.Application.AgencyTiers.Queries.GetAgencyTierById;

public record GetAgencyTierByIdQuery : IQuery<AgencyTierResponse>
{
    public Guid AgencyTierId { get; init; }
} 