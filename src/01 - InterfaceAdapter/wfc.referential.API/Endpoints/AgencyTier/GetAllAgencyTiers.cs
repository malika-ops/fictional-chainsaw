using BuildingBlocks.Core.Pagination;
using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.AgencyTiers.Dtos;
using wfc.referential.Application.AgencyTiers.Queries.GetAllAgencyTiers;

namespace wfc.referential.API.Endpoints.AgencyTier;

public class GetAllAgencyTiers(IMediator _mediator)
    : Endpoint<GetAllAgencyTiersRequest, PagedResult<AgencyTierResponse>>
{
    public override void Configure()
    {
        Get("/api/agencyTiers");
        AllowAnonymous();

        Summary(s =>
        {
            s.Summary = "Get paginated list of Agency-Tier mappings";
            s.Description = "Returns a paginated list of AgencyTiers. "
                          + "Supports optional filtering by AgencyId, TierId, Code and IsEnabled.";

            s.Response<PagedResult<AgencyTierResponse>>(200, "Paged list of AgencyTiers");
            s.Response(400, "Invalid pageNumber/pageSize etc.");
            s.Response(500, "Unexpected server error");
        });

        Options(o => o.WithTags(EndpointGroups.AgencyTiers));
    }

    public override async Task HandleAsync(GetAllAgencyTiersRequest req, CancellationToken ct)
    {
        var query = req.Adapt<GetAllAgencyTiersQuery>();
        var result = await _mediator.Send(query, ct);
        await SendAsync(result, cancellation: ct);
    }
}