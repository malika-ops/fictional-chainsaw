using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using Mapster;
using wfc.referential.Application.Interfaces;
using wfc.referential.Application.MonetaryZones.Dtos;

namespace wfc.referential.Application.MonetaryZones.Queries.GetFiltredMonetaryZones;

public class GetFiltredMonetaryZonesQueryHandler : IQueryHandler<GetFiltredMonetaryZonesQuery, PagedResult<MonetaryZoneResponse>>
{
    private readonly IMonetaryZoneRepository _monetaryZoneRepository;

    public GetFiltredMonetaryZonesQueryHandler(IMonetaryZoneRepository monetaryZoneRepository)
    {
        _monetaryZoneRepository = monetaryZoneRepository;
    }

    public async Task<PagedResult<MonetaryZoneResponse>> Handle(GetFiltredMonetaryZonesQuery request, CancellationToken cancellationToken)
    {

        var monetaryZones = await _monetaryZoneRepository.GetPagedByCriteriaAsync(request,
            request.PageNumber,
            request.PageSize,
            cancellationToken, 
            m => m.Countries);

        return new PagedResult<MonetaryZoneResponse>(monetaryZones.Items.Adapt<List<MonetaryZoneResponse>>(),
            monetaryZones.TotalCount,
            monetaryZones.PageNumber,
            monetaryZones.PageSize);
    }
}
