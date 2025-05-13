using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.Corridors.Commands.UpdateCorridor;
using wfc.referential.Application.Corridors.Dtos;

namespace wfc.referential.API.Endpoints.Corridor;

public class UpdateCorridor(IMediator _mediator) : Endpoint<UpdateCorridorRequest, Guid>
{
    public override void Configure()
    {
        Put("api/corridors/{CorridorId}");
        AllowAnonymous();

        Summary(s =>
        {
            s.Summary = "Fully update a Corridor's properties";
            s.Description = "Updates all fields of the specified Corridor ID.";
            s.Params["CorridorId"] = "Corridor ID (GUID) from route";

            s.Response<Guid>(200, "The ID of the updated Corridor");
            s.Response(400, "If validation fails or the ID is invalid");
            s.Response(404, "Corridor not found");
        });

        Options(o => o.WithTags(EndpointGroups.Corridors));
    }

    public override async Task HandleAsync(UpdateCorridorRequest req, CancellationToken ct)
    {
        var command = req.Adapt<UpdateCorridorCommand>();

        var updatedId = await _mediator.Send(command, ct);

        await SendAsync(updatedId.Value, cancellation: ct);
    }
}
