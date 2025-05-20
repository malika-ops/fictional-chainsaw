using BuildingBlocks.Core.Pagination;
using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.Countries.Dtos;
using wfc.referential.Application.Countries.Queries.GetAllCounties;

namespace wfc.referential.API.Endpoints.Country;

public class GetAllCountriesEndpoint(IMediator _mediator) : Endpoint<GetAllCountriesRequest, PagedResult<GetCountriesResponce>>
{
    public override void Configure()
    {
        Get("/api/countries");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Get paginated list of countries";
            s.Description = "Returns a paginated list of countries. Supports optional filtering by abbreviation, name, code, ISO2, ISO3, dialing code, time zone, and status.";
            s.Response<PagedResult<GetCountriesResponce>>(200, "Successful Response");
            s.Response(400, "If the request is invalid, e.g. invalid pagination values");
            s.Response(500, "Server error if an unexpected error occurs");
        });
        Options(o => o.WithTags(EndpointGroups.Countries));
    }

    public override async Task HandleAsync(GetAllCountriesRequest requestObject, CancellationToken ct)
    {
        var query = requestObject.Adapt<GetAllCountriesQuery>();

        var result = await _mediator.Send(query, ct);

        await SendAsync(result, cancellation: ct);
    }
}