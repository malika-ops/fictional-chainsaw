using BuildingBlocks.Core.Pagination;
using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.CountryServices.Dtos;
using wfc.referential.Application.CountryServices.Queries.GetAllCountryServices;

namespace wfc.referential.API.Endpoints.CountryServices;

public class GetAllCountryServices(IMediator _mediator)
    : Endpoint<GetAllCountryServicesRequest, PagedResult<GetCountryServicesResponse>>
{
    public override void Configure()
    {
        Get("/api/countryservices");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Get paginated list of country-services document associations";
            s.Description = """
                Returns a paginated list of country-services document associations.
                Filters supported: countryId, serviceId, status.
                """;
            s.Response<PagedResult<GetCountryServicesResponse>>(200, "Successful response");
            s.Response(400, "Invalid pagination/filter parameters");
            s.Response(500, "Server error");
        });
        Options(o => o.WithTags(EndpointGroups.CountryServices));
    }

    public override async Task HandleAsync(GetAllCountryServicesRequest req, CancellationToken ct)
    {
        var query = req.Adapt<GetAllCountryServicesQuery>();
        var result = await _mediator.Send(query, ct);
        await SendAsync(result, cancellation: ct);
    }
}