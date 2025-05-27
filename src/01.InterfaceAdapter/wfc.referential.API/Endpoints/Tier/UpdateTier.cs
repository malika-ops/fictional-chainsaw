using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.Tiers.Commands.UpdateTier;
using wfc.referential.Application.Tiers.Dtos;

namespace wfc.referential.API.Endpoints.Tier;

public class UpdateTier(IMediator _mediator)
    : Endpoint<UpdateTierRequest, bool>
{
    public override void Configure()
    {
        Put("/api/tiers/{TierId}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Update an existing Tier";
            s.Description = "Replaces Name, Description and IsEnabled of the specified Tier.";
            s.Params["TierId"] = "Tier ID (GUID) from route";

            s.Response<bool>(200, "Returns a boolean (True or False) as the status of the update");
            s.Response(400, "Validation or business rule failed");
            s.Response(404, "Tier not found");
            s.Response(409, "Conflict with an existing Tier");
        });
        Options(o => o.WithTags(EndpointGroups.Tiers));
    }

    public override async Task HandleAsync(UpdateTierRequest dto,
                                           CancellationToken ct)
    {
        var cmd = dto.Adapt<UpdateTierCommand>();
        var result = await _mediator.Send(cmd, ct);
        await SendAsync(result.Value, cancellation: ct);
    }
}