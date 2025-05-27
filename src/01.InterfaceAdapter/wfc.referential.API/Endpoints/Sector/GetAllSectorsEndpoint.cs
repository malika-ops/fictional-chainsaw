using BuildingBlocks.Core.Pagination;
using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.Sectors.Dtos;
using wfc.referential.Application.Sectors.Queries.GetAllSectors;

namespace wfc.referential.API.Endpoints.Sector;

public class GetAllSectorsEndpoint(IMediator _mediator)
    : Endpoint<GetAllSectorsRequest, PagedResult<SectorResponse>>
{
    public override void Configure()
    {
        Get("/api/sectors");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Get paginated list of sectors";
            s.Description = """
                Returns a paginated list of sectors.
                Filters supported: code, name, cityId, status.
                """;
            s.Response<PagedResult<SectorResponse>>(200, "Successful response");
            s.Response(400, "Invalid pagination/filter parameters");
            s.Response(500, "Server error");
        });
        Options(o => o.WithTags(EndpointGroups.Sectors));
    }

    public override async Task HandleAsync(GetAllSectorsRequest req, CancellationToken ct)
    {
        var query = req.Adapt<GetAllSectorsQuery>();
        var result = await _mediator.Send(query, ct);
        await SendAsync(result, cancellation: ct);
    }
}