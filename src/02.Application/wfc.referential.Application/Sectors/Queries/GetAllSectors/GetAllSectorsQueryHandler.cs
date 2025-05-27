using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using Mapster;
using wfc.referential.Application.Interfaces;
using wfc.referential.Application.Sectors.Dtos;

namespace wfc.referential.Application.Sectors.Queries.GetAllSectors;

public class GetAllSectorsQueryHandler
    : IQueryHandler<GetAllSectorsQuery, PagedResult<SectorResponse>>
{
    private readonly ISectorRepository _repo;

    public GetAllSectorsQueryHandler(ISectorRepository repo) => _repo = repo;

    public async Task<PagedResult<SectorResponse>> Handle(
        GetAllSectorsQuery sectorQuery, CancellationToken ct)
    {
        var sectors = await _repo.GetPagedByCriteriaAsync(sectorQuery, sectorQuery.PageNumber, sectorQuery.PageSize, ct);
        return new PagedResult<SectorResponse>(sectors.Items.Adapt<List<SectorResponse>>(), sectors.TotalCount, sectors.PageNumber, sectors.PageSize);
    }
}