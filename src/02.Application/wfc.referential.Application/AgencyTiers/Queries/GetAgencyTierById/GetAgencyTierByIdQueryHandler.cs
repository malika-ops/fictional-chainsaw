using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Exceptions;
using Mapster;
using wfc.referential.Application.AgencyTiers.Dtos;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.AgencyTierAggregate;

namespace wfc.referential.Application.AgencyTiers.Queries.GetAgencyTierById;

public class GetAgencyTierByIdQueryHandler : IQueryHandler<GetAgencyTierByIdQuery, AgencyTierResponse>
{
    private readonly IAgencyTierRepository _agencyTierRepository;

    public GetAgencyTierByIdQueryHandler(IAgencyTierRepository agencyTierRepository)
    {
        _agencyTierRepository = agencyTierRepository;
    }

    public async Task<AgencyTierResponse> Handle(GetAgencyTierByIdQuery query, CancellationToken ct)
    {
        var id = AgencyTierId.Of(query.AgencyTierId);
        var entity = await _agencyTierRepository.GetByIdAsync(id, ct)
            ?? throw new ResourceNotFoundException($"AgencyTier with id '{query.AgencyTierId}' not found.");

        return entity.Adapt<AgencyTierResponse>();
    }
} 