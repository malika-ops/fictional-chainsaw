using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.Corridors.Commands.DeleteCorridor;
using wfc.referential.Application.Corridors.Dtos;

namespace wfc.referential.API.Endpoints.Corridor;

public class DeleteCorridor(IMediator _mediator) : Endpoint<DeleteCorridorRequest, bool>
{
    public override void Configure()
    {
        Delete("api/corridors/{CorridorId}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Delete a Corridor";
            s.Params["CorridorId"] = "The string representation of the Corridor GUID.";
        });
        Options(o => o.WithTags(EndpointGroups.Corridors));
    }

    public override async Task HandleAsync(DeleteCorridorRequest req, CancellationToken ct)
    {
        var command = req.Adapt<DeleteCorridorCommand>();
        var result = await _mediator.Send(command, ct);
        await SendAsync(result.Value, cancellation: ct);
    }
}