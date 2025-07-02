using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Exceptions;
using Mapster;
using wfc.referential.Application.MonetaryZones.Dtos;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.MonetaryZoneAggregate;

namespace wfc.referential.Application.MonetaryZones.Queries.GetMonetaryZoneById;

public class GetMonetaryZoneByIdQueryHandler : IQueryHandler<GetMonetaryZoneByIdQuery, MonetaryZoneResponse>
{
    private readonly IMonetaryZoneRepository _monetaryZoneRepository;

    public GetMonetaryZoneByIdQueryHandler(IMonetaryZoneRepository monetaryZoneRepository)
    {
        _monetaryZoneRepository = monetaryZoneRepository;
    }

    public async Task<MonetaryZoneResponse> Handle(GetMonetaryZoneByIdQuery query, CancellationToken ct)
    {
        var id = MonetaryZoneId.Of(query.MonetaryZoneId);
        var entity = await _monetaryZoneRepository.GetByIdAsync(id, ct)
            ?? throw new ResourceNotFoundException($"MonetaryZone with id '{query.MonetaryZoneId}' not found.");

        return entity.Adapt<MonetaryZoneResponse>();
    }
} 