using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.MonetaryZones.Commands.DeleteMonetaryZone;
using wfc.referential.Application.MonetaryZones.Dtos;

namespace wfc.referential.API.Endpoints.MonetaryZonesEndpoints;

public class DeleteMonetaryZone(IMediator _mediator) : Endpoint<DeleteMonetaryZoneRequest, bool>
{
    public override void Configure()
    {
        Delete("/api/monetaryZones/{MonetaryZoneId}");
        AllowAnonymous();

        Summary(s =>
        {
            s.Summary = "Delete a MonetaryZone by GUID";
            s.Description = "Deletes the MonetaryZone identified by {MonetaryZoneId}, as route param.";

            s.Response<bool>(200, "True if deletion succeeded");
            s.Response(400, "If if deletion failed");
        });

        Options(o => o.WithTags(EndpointGroups.MonetaryZones));
    }

    public override async Task HandleAsync(DeleteMonetaryZoneRequest dto, CancellationToken ct)
    {
        var command = dto.Adapt<DeleteMonetaryZoneCommand>();
        var result = await _mediator.Send(command, ct);
        await SendAsync(result.Value, cancellation: ct);
    }
}
