using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.MonetaryZones.Commands.UpdateMonetaryZone;
using wfc.referential.Application.MonetaryZones.Dtos;


namespace wfc.referential.API.Endpoints.MonetaryZoneEndpoints;
public class UpdateMonetaryZone(IMediator _mediator) : Endpoint<UpdateMonetaryZoneRequest, Guid>
{
    public override void Configure()
    {
        Put("/api/monetaryZones/{MonetaryZoneId}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Update an existing MonetaryZone";
            s.Description = "Updates the zone identified by MonetaryZoneId with new code, name, description.";
            s.Params["MonetaryZoneId"] = "MonetaryZone ID (GUID) from route";

            s.Response<Guid>(200, "Returns the ID of the updated zone upon success");
            s.Response(400, "Returned if validation fails (missing fields, invalid ID, etc.)");
            s.Response(500, "Server error if something unexpected occurs");
        });
        Options(o => o.WithTags(EndpointGroups.MonetaryZones));
    }

    public override async Task HandleAsync(UpdateMonetaryZoneRequest dto, CancellationToken ct)
    {
        var command = dto.Adapt<UpdateMonetaryZoneCommand>();
        var result = await _mediator.Send(command, ct);
        await SendAsync(result.Value, cancellation: ct);
    }
}