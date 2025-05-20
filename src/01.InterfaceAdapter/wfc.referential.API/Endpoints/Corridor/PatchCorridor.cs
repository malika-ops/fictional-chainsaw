using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.Corridors.Commands.PatchCorridor;
using wfc.referential.Application.Corridors.Dtos;


namespace wfc.referential.API.Endpoints.Corridor;

public class PatchCorridor(IMediator _mediator) : Endpoint<PatchCorridorRequest, Guid>
{
    public override void Configure()
    {
        Patch("api/corridors/{CorridorId}"); 
        AllowAnonymous();

        Summary(s =>
        {
            s.Summary = "Partially update a Corridor's properties";
            s.Description = "Updates only the provided fields of the specified Corridor ID.";
            s.Params["CorridorId"] = "Corridor ID (GUID) from route";

            s.Response<Guid>(200, "The ID of the updated Corridor");
            s.Response(400, "If validation fails or the ID is invalid");
            s.Response(404, "Corridor not found");
        });

        Options(o => o.WithTags(EndpointGroups.Corridors));
    }

    public override async Task HandleAsync(PatchCorridorRequest req, CancellationToken ct)
    {
        var command = req.Adapt<PatchCorridorCommand>();

        var updatedId = await _mediator.Send(command, ct);

        await SendAsync(updatedId.Value, cancellation: ct);
    }
}
