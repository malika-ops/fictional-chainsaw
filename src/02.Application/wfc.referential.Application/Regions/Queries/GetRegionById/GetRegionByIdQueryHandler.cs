using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Exceptions;
using Mapster;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.RegionAggregate;
using wfc.referential.Application.RegionManagement.Dtos;

namespace wfc.referential.Application.Regions.Queries.GetRegionById;

public class GetRegionByIdQueryHandler : IQueryHandler<GetRegionByIdQuery, GetRegionsResponse>
{
    private readonly IRegionRepository _regionRepository;

    public GetRegionByIdQueryHandler(IRegionRepository regionRepository)
    {
        _regionRepository = regionRepository;
    }

    public async Task<GetRegionsResponse> Handle(GetRegionByIdQuery query, CancellationToken ct)
    {
        var id = RegionId.Of(query.RegionId);
        var entity = await _regionRepository.GetByIdAsync(id, ct)
            ?? throw new ResourceNotFoundException($"Region with id '{query.RegionId}' not found.");

        return entity.Adapt<GetRegionsResponse>();
    }
} 