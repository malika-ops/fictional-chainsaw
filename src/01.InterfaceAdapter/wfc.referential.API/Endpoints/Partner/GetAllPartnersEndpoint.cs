using BuildingBlocks.Core.Pagination;
using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.Partners.Dtos;
using wfc.referential.Application.Partners.Queries.GetAllPartners;

namespace wfc.referential.API.Endpoints.Partner;

public class GetAllPartnersEndpoint(IMediator _mediator)
    : Endpoint<GetAllPartnersRequest, PagedResult<GetPartnersResponse>>
{
    public override void Configure()
    {
        Get("/api/partners");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Get paginated list of partners";
            s.Description = """
                Returns a paginated list of partners.
                Filters supported: code, name, person type, professional tax number, headquarters city, tax identification number, ICE, status.
                """;
            s.Response<PagedResult<GetPartnersResponse>>(200, "Successful response");
            s.Response(400, "Invalid pagination/filter parameters");
            s.Response(500, "Server error");
        });

        Options(o => o.WithTags(EndpointGroups.Partners));
    }

    public override async Task HandleAsync(GetAllPartnersRequest req, CancellationToken ct)
    {
        var query = req.Adapt<GetAllPartnersQuery>();
        var result = await _mediator.Send(query, ct);
        await SendAsync(result, cancellation: ct);
    }
}