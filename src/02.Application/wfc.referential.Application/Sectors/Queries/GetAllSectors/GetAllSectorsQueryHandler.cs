using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using Mapster;
using wfc.referential.Application.Interfaces;
using wfc.referential.Application.Sectors.Dtos;

namespace wfc.referential.Application.Sectors.Queries.GetAllSectors;

public class GetAllSectorsQueryHandler : IQueryHandler<GetAllSectorsQuery, PagedResult<SectorResponse>>
{
    private readonly ISectorRepository _sectorRepository;

    public GetAllSectorsQueryHandler(ISectorRepository sectorRepository)
    {
        _sectorRepository = sectorRepository;
    }

    public async Task<PagedResult<SectorResponse>> Handle(GetAllSectorsQuery request, CancellationToken cancellationToken)
    {
             var sectors = await _sectorRepository
            .GetFilteredSectorsAsync(request, cancellationToken);

        int totalCount = await _sectorRepository
            .GetCountTotalAsync(request, cancellationToken);

        var sectorResponses = sectors.Adapt<List<SectorResponse>>();

        return new PagedResult<SectorResponse>(sectorResponses, totalCount, request.PageNumber, request.PageSize);
    }
}