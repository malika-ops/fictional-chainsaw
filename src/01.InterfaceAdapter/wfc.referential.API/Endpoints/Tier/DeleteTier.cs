using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.Tiers.Commands.DeleteTier;
using wfc.referential.Application.Tiers.Dtos;

namespace wfc.referential.API.Endpoints.Tier;

public class DeleteTier(IMediator _mediator) : Endpoint<DeleteTierRequest, bool>
{
    public override void Configure()
    {
        Delete("/api/tiers/{TierId}");
        AllowAnonymous();

        Summary(s =>
        {
            s.Summary = "Delete a Tier by GUID";
            s.Description = "Soft-deletes (disables) the Tier identified by the route parameter.";
            s.Params["TierId"] = "Tier ID (GUID)";
            s.Response<bool>(200, "True if deletion succeeded");
            s.Response(400, "Validation / business rules failed");
        });

        Options(o => o.WithTags(EndpointGroups.Tiers));
    }

    public override async Task HandleAsync(DeleteTierRequest dto, CancellationToken ct)
    {
        var cmd = dto.Adapt<DeleteTierCommand>();
        var result = await _mediator.Send(cmd, ct);
        await SendAsync(result.Value, cancellation: ct);
    }
}