using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Pagination;
using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.MonetaryZones.Dtos;
using wfc.referential.Application.MonetaryZones.Queries.GetAllMonetaryZones;

namespace wfc.referential.API.Endpoints.MonetaryZoneEndpoints;

public class GetAllMonetaryZone(IMediator _mediator) : Endpoint<GetAllMonetaryZonesRequest, PagedResult<MonetaryZoneResponse>>
{
    public override void Configure()
    {
        Get("/api/monetaryZones");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Get paginated list of MonetaryZones";
            s.Description = "Returns a paginated list of MonetaryZones. Supports optional filtering by code, name, and description.";

            s.Response<PagedResult<MonetaryZoneResponse>>(200, "Paged list of MonetaryZones");
            s.Response(400, "If validation fails, e.g. invalid pageNumber/pageSize");
            s.Response(500, "Server error if unexpected");
        });

        Options(o => o.WithTags(EndpointGroups.MonetaryZones));
    }

    public override async Task HandleAsync(GetAllMonetaryZonesRequest requestObject, CancellationToken ct)
    {
        var query = requestObject.Adapt<GetAllMonetaryZonesQuery>();

        var result = await _mediator.Send(query, ct);

        await SendAsync(result, cancellation: ct);
    }
}