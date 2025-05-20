using BuildingBlocks.Core.Pagination;
using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.Sectors.Dtos;
using wfc.referential.Application.Sectors.Queries.GetAllSectors;

namespace wfc.referential.API.Endpoints.Sector;

public class GetAllSectors(IMediator _mediator) : Endpoint<GetAllSectorsRequest, PagedResult<SectorResponse>>
{
    public override void Configure()
    {
        Get("/api/sectors");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Get paginated list of Sectors";
            s.Description = "Returns a paginated list of Sectors. Supports optional filtering by code, name, country, city, region, and status.";

            s.Response<PagedResult<SectorResponse>>(200, "Paged list of Sectors");
            s.Response(400, "If validation fails, e.g. invalid pageNumber/pageSize");
            s.Response(500, "Server error if unexpected");
        });

        Options(o => o.WithTags(EndpointGroups.Sectors));
    }

    public override async Task HandleAsync(GetAllSectorsRequest requestObject, CancellationToken ct)
    {
        var query = requestObject.Adapt<GetAllSectorsQuery>();
        var result = await _mediator.Send(query, ct);
        await SendAsync(result, cancellation: ct);
    }
}