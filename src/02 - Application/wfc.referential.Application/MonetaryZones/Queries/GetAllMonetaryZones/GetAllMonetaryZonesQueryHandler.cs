using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using Mapster;
using wfc.referential.Application.Interfaces;
using wfc.referential.Application.MonetaryZones.Dtos;

namespace wfc.referential.Application.MonetaryZones.Queries.GetAllMonetaryZones;

public class GetAllMonetaryZonesQueryHandler : IQueryHandler<GetAllMonetaryZonesQuery, PagedResult<MonetaryZoneResponse>>
{
    private readonly IMonetaryZoneRepository _monetaryZoneRepository;

    public GetAllMonetaryZonesQueryHandler(IMonetaryZoneRepository monetaryZoneRepository)
    {
        _monetaryZoneRepository = monetaryZoneRepository;
    }

    public async Task<PagedResult<MonetaryZoneResponse>> Handle(GetAllMonetaryZonesQuery request, CancellationToken cancellationToken)
    {

        var monetaryZones = await _monetaryZoneRepository
        .GetFilteredMonetaryZonesAsync(request, cancellationToken);

        int totalCount = await _monetaryZoneRepository
            .GetCountTotalAsync(request, cancellationToken);

        var monetaryZoneDtos = monetaryZones.Adapt<List<MonetaryZoneResponse>>();

        return new PagedResult<MonetaryZoneResponse>(monetaryZoneDtos, totalCount, request.PageNumber, request.PageSize);
    }
}
