using BuildingBlocks.Core.Pagination;
using FastEndpoints;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using wfc.referential.Application.RegionManagement.Dtos;
using wfc.referential.Application.RegionManagement.Queries.GetAllRegions;

namespace wfc.referential.API.Endpoints.Region;

public class GetAllRegions(IMediator _mediator) : Endpoint<GetAllRegionsRequest, PagedResult<GetAllRegionsResponse>>
{
    public override void Configure()
    {
        Get("api/regions");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Get paginated list of regions";
            s.Response<PagedResult<GetAllRegionsResponse>>(StatusCodes.Status200OK, "Successful Response");
            s.Response<BadRequest>(StatusCodes.Status400BadRequest,"If validation fails, e.g. invalid pageNumber/pageSize");
            s.Response<InternalErrorResponse>(StatusCodes.Status500InternalServerError, "Server error if unexpected");
        });
        Options(o => o.WithTags(EndpointGroups.Regions));
    }

    public override async Task HandleAsync(GetAllRegionsRequest req, CancellationToken ct)
    {
        var query = req.Adapt<GetAllRegionsQuery>();

        var result = await _mediator.Send(query, ct);

        await SendAsync(result, cancellation: ct);
    }
}