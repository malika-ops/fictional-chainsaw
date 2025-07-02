using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Exceptions;
using Mapster;
using wfc.referential.Application.Sectors.Dtos;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.SectorAggregate;

namespace wfc.referential.Application.Sectors.Queries.GetSectorById;

public class GetSectorByIdQueryHandler : IQueryHandler<GetSectorByIdQuery, SectorResponse>
{
    private readonly ISectorRepository _sectorRepository;

    public GetSectorByIdQueryHandler(ISectorRepository sectorRepository)
    {
        _sectorRepository = sectorRepository;
    }

    public async Task<SectorResponse> Handle(GetSectorByIdQuery query, CancellationToken ct)
    {
        var id = SectorId.Of(query.SectorId);
        var entity = await _sectorRepository.GetByIdAsync(id, ct)
            ?? throw new ResourceNotFoundException($"Sector with id '{query.SectorId}' not found.");

        return entity.Adapt<SectorResponse>();
    }
} 