using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Exceptions;
using Mapster;
using wfc.referential.Application.Corridors.Dtos;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.CorridorAggregate;

namespace wfc.referential.Application.Corridors.Queries.GetCorridorById;

public class GetCorridorByIdQueryHandler : IQueryHandler<GetCorridorByIdQuery, GetCorridorResponse>
{
    private readonly ICorridorRepository _corridorRepository;

    public GetCorridorByIdQueryHandler(ICorridorRepository corridorRepository)
    {
        _corridorRepository = corridorRepository;
    }

    public async Task<GetCorridorResponse> Handle(GetCorridorByIdQuery query, CancellationToken ct)
    {
        var id = CorridorId.Of(query.CorridorId);
        var entity = await _corridorRepository.GetByIdAsync(id, ct)
            ?? throw new ResourceNotFoundException($"Corridor with id '{query.CorridorId}' not found.");

        return entity.Adapt<GetCorridorResponse>();
    }
} 