using BuildingBlocks.Core.Pagination;
using FastEndpoints;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using wfc.referential.Application.Corridors.Dtos;
using wfc.referential.Application.Corridors.Queries.GetAllCorridors;

namespace wfc.referential.API.Endpoints.Corridor;

public class GetAllCorridors(IMediator _mediator) : Endpoint<GetAllCorridorsRequest, PagedResult<GetAllCorridorsResponse>>
{
    public override void Configure()
    {
        Get($"api/{EndpointGroups.Corridors}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Get paginated list of corridors";
            s.Response<PagedResult<GetAllCorridorsResponse>>(StatusCodes.Status200OK, "Successful Response");
            s.Response<BadRequest>(StatusCodes.Status400BadRequest,"If validation fails, e.g. invalid pageNumber/pageSize");
            s.Response<InternalErrorResponse>(StatusCodes.Status500InternalServerError, "Server error if unexpected");
        });
        Options(o => o.WithTags(EndpointGroups.Corridors));
    }

    public override async Task HandleAsync(GetAllCorridorsRequest req, CancellationToken ct)
    {
        var query = req.Adapt<GetAllCorridorsQuery>();

        var result = await _mediator.Send(query, ct);

        await SendAsync(result, cancellation: ct);
    }
}