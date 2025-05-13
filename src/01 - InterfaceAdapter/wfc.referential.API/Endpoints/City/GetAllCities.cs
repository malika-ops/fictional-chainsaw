using BuildingBlocks.Core.Pagination;
using FastEndpoints;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using wfc.referential.Application.Cities.Dtos;
using wfc.referential.Application.Cities.Queries.GetAllCities;

namespace wfc.referential.API.Endpoints.City;

public class GetAllCities(IMediator _mediator) : Endpoint<GetAllCitiesRequest, PagedResult<GetAllCitiesResponse>>
{
    public override void Configure()
    {
        Get("api/cities");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Get paginated list of cities";
            s.Response<PagedResult<GetAllCitiesResponse>>(StatusCodes.Status200OK, "Successful Response");
            s.Response<BadRequest>(StatusCodes.Status400BadRequest,"If validation fails, e.g. invalid pageNumber/pageSize");
            s.Response<InternalErrorResponse>(StatusCodes.Status500InternalServerError, "Server error if unexpected");
        });
        Options(o => o.WithTags(EndpointGroups.Cities));
    }

    public override async Task HandleAsync(GetAllCitiesRequest req, CancellationToken ct)
    {
        var query = req.Adapt<GetAllCitiesQuery>();

        var result = await _mediator.Send(query, ct);

        await SendAsync(result, cancellation: ct);
    }
}