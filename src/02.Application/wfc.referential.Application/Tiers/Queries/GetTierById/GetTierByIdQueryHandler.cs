using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Exceptions;
using Mapster;
using wfc.referential.Application.Tiers.Dtos;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.TierAggregate;

namespace wfc.referential.Application.Tiers.Queries.GetTierById;

public class GetTierByIdQueryHandler : IQueryHandler<GetTierByIdQuery, TierResponse>
{
    private readonly ITierRepository _tierRepository;

    public GetTierByIdQueryHandler(ITierRepository tierRepository)
    {
        _tierRepository = tierRepository;
    }

    public async Task<TierResponse> Handle(GetTierByIdQuery query, CancellationToken ct)
    {
        var id = TierId.Of(query.TierId);
        var entity = await _tierRepository.GetByIdAsync(id, ct)
            ?? throw new ResourceNotFoundException($"Tier with id '{query.TierId}' not found.");

        return entity.Adapt<TierResponse>();
    }
} 