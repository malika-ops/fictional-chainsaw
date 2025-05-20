using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.MonetaryZones.Commands.PatchMonetaryZone;
using wfc.referential.Application.MonetaryZones.Dtos;

namespace wfc.referential.API.Endpoints.MonetaryZonesEndpoints;

public class PatchMonetaryZone(IMediator _mediator) : Endpoint<PatchMonetaryZoneRequest, Guid>
{
    public override void Configure()
    {
        Patch("/patchMonetaryZone/{MonetaryZoneId}"); 
        AllowAnonymous();

        Summary(s =>
        {
            s.Summary = "Partially update a MonetaryZone's properties";
            s.Description = "Updates only the provided fields (code, name, or description) of the specified zone ID.";
            s.Params["MonetaryZoneId"] = "MonetaryZone ID (GUID) from route";

            s.Response<Guid>(200, "The ID of the updated zone");
            s.Response(400, "If validation fails or the ID is invalid");
            s.Response(404, "Zone not found");
        });

        Options(o => o.WithTags(EndpointGroups.MonetaryZones));
    }

    public override async Task HandleAsync(PatchMonetaryZoneRequest req, CancellationToken ct)
    {
        var command = req.Adapt<PatchMonetaryZoneCommand>();

        var result = await _mediator.Send(command, ct);

        await SendAsync(result.Value, cancellation: ct);
    }
}
