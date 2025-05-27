using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.Tiers.Commands.PatchTier;
using wfc.referential.Application.Tiers.Dtos;

namespace wfc.referential.API.Endpoints.Tier;

public class PatchTier(IMediator _mediator) : Endpoint<PatchTierRequest, bool>
{
    public override void Configure()
    {
        Patch("/api/tiers/{TierId}");
        AllowAnonymous();

        Summary(s =>
        {
            s.Summary = "Partially update a Tier";
            s.Description = "Updates only the supplied fields (Name, Description, IsEnabled), ";
            s.Params["TierId"] = "Tier ID (GUID) from route";
            s.Response<bool>(200, "Returns a boolean (True or False) as the status of the patch");
            s.Response(400, "Validation / business rules failed");
            s.Response(404, "Tier not found");
            s.Response(409, "Conflict with an existing Tier");
        });

        Options(o => o.WithTags(EndpointGroups.Tiers));
    }

    public override async Task HandleAsync(PatchTierRequest req, CancellationToken ct)
    {
        var cmd = req.Adapt<PatchTierCommand>();
        var result = await _mediator.Send(cmd, ct);
        await SendAsync(result.Value, cancellation: ct);
    }
}