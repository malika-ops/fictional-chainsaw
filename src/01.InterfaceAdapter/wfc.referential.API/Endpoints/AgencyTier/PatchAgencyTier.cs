using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.AgencyTiers.Commands.PatchAgencyTier;
using wfc.referential.Application.AgencyTiers.Dtos;

namespace wfc.referential.API.Endpoints.AgencyTier;

public class PatchAgencyTier(IMediator _mediator)
        : Endpoint<PatchAgencyTierRequest, bool>
{
    public override void Configure()
    {
        Patch("/api/agencyTiers/{AgencyTierId}");
        AllowAnonymous();

        Summary(s =>
        {
            s.Summary = "Partially update an Agency-Tier link";
            s.Description = "Only the supplied fields are updated (Code, Password, IsEnabled, "
                          + "AgencyId, TierId).";
            s.Params["AgencyTierId"] = " Returns a boolean (True or False) as the status of the patch";
            s.Response<bool>(200, "ID of the updated AgencyTier");
            s.Response(400, "Validation errors");
            s.Response(404, "AgencyTier not found");
            s.Response(409, "Conflict with an existing AgencyTier");
        });

        Options(o => o.WithTags(EndpointGroups.AgencyTiers));
    }

    public override async Task HandleAsync(PatchAgencyTierRequest req, CancellationToken ct)
    {
        var command = req.Adapt<PatchAgencyTierCommand>();
        var result = await _mediator.Send(command, ct);
        await SendAsync(result.Value, cancellation: ct);
    }
}