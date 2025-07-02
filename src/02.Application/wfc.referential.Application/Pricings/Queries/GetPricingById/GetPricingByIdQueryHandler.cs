using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Exceptions;
using Mapster;
using wfc.referential.Application.Pricings.Dtos;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.PricingAggregate;

namespace wfc.referential.Application.Pricings.Queries.GetPricingById;

public class GetPricingByIdQueryHandler : IQueryHandler<GetPricingByIdQuery, PricingResponse>
{
    private readonly IPricingRepository _pricingRepository;

    public GetPricingByIdQueryHandler(IPricingRepository pricingRepository)
    {
        _pricingRepository = pricingRepository;
    }

    public async Task<PricingResponse> Handle(GetPricingByIdQuery query, CancellationToken ct)
    {
        var id = PricingId.Of(query.PricingId);
        var entity = await _pricingRepository.GetByIdAsync(id, ct)
            ?? throw new ResourceNotFoundException($"Pricing with id '{query.PricingId}' not found.");

        return entity.Adapt<PricingResponse>();
    }
} 