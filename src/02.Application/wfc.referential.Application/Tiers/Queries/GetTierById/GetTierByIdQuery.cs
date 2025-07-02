using BuildingBlocks.Core.Abstraction.CQRS;
using wfc.referential.Application.Tiers.Dtos;

namespace wfc.referential.Application.Tiers.Queries.GetTierById;

public record GetTierByIdQuery : IQuery<TierResponse>
{
    public Guid TierId { get; init; }
} 