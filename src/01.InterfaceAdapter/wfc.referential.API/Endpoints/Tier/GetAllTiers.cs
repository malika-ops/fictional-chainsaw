using BuildingBlocks.Core.Pagination;
using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.Tiers.Dtos;
using wfc.referential.Application.Tiers.Queries.GetAllTiers;

namespace wfc.referential.API.Endpoints.Tier;

public class GetAllTiers(IMediator _mediator) : Endpoint<GetAllTiersRequest, PagedResult<TierResponse>>
{
    public override void Configure()
    {
        Get("/api/tiers");
        AllowAnonymous();

        Summary(s =>
        {
            s.Summary = "Get paginated list of Tiers";
            s.Description = "Returns a paginated list of tiers with optional filters on name, description and enabled flag.";
            s.Response<PagedResult<TierResponse>>(200, "Paged list of tiers");
            s.Response(400, "Invalid pagination/filter parameters");
            s.Response(500, "Server error");
        });

        Options(o => o.WithTags(EndpointGroups.Tiers));
    }

    public override async Task HandleAsync(GetAllTiersRequest req, CancellationToken ct)
    {
        var query = req.Adapt<GetAllTiersQuery>();
        var result = await _mediator.Send(query, ct);
        await SendAsync(result, cancellation: ct);
    }
}