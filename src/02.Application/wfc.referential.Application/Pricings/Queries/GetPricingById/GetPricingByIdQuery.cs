using BuildingBlocks.Core.Abstraction.CQRS;
using wfc.referential.Application.Pricings.Dtos;

namespace wfc.referential.Application.Pricings.Queries.GetPricingById;

public record GetPricingByIdQuery : IQuery<PricingResponse>
{
    public Guid PricingId { get; init; }
} 