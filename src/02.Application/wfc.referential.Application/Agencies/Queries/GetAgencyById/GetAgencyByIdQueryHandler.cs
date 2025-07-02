using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Exceptions;
using Mapster;
using wfc.referential.Application.Agencies.Dtos;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.AgencyAggregate;

namespace wfc.referential.Application.Agencies.Queries.GetAgencyById;

public class GetAgencyByIdQueryHandler : IQueryHandler<GetAgencyByIdQuery, GetAgenciesResponse>
{
    private readonly IAgencyRepository _agencyRepository;

    public GetAgencyByIdQueryHandler(IAgencyRepository agencyRepository)
    {
        _agencyRepository = agencyRepository;
    }

    public async Task<GetAgenciesResponse> Handle(GetAgencyByIdQuery query, CancellationToken ct)
    {
        var id = AgencyId.Of(query.AgencyId);
        var entity = await _agencyRepository.GetByIdAsync(id, ct)
            ?? throw new ResourceNotFoundException($"Agency with id '{query.AgencyId}' not found.");

        return entity.Adapt<GetAgenciesResponse>();
    }
} 