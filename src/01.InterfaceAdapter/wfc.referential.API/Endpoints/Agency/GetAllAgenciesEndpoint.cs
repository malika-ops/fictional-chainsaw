using BuildingBlocks.Core.Pagination;
using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.Agencies.Dtos;
using wfc.referential.Application.Agencies.Queries.GetAllAgencies;

namespace wfc.referential.API.Endpoints.Agency;

public class GetAllAgenciesEndpoint(IMediator _mediator)
    : Endpoint<GetAllAgenciesRequest, PagedResult<GetAgenciesResponse>>
{
    public override void Configure()
    {
        Get("/api/agencies");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Get paginated list of agencies";
            s.Description = """
                Returns a paginated list of agencies.
                Filters supported: code, name, abbreviation, phone, fax, accounting fields,
                postal code, CityId, SectorId, AgencyType (id, value, libellé), status.
                """;
            s.Response<PagedResult<GetAgenciesResponse>>(200, "Successful response");
            s.Response(400, "Invalid pagination/filter parameters");
            s.Response(500, "Server error");
        });
        Options(o => o.WithTags(EndpointGroups.Agencies));
    }

    public override async Task HandleAsync(GetAllAgenciesRequest req, CancellationToken ct)
    {
        var query = req.Adapt<GetAllAgenciesQuery>();
        var result = await _mediator.Send(query, ct);
        await SendAsync(result, cancellation: ct);
    }
}
