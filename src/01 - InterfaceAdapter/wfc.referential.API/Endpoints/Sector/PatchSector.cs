using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.Sectors.Commands.PatchSector;
using wfc.referential.Application.Sectors.Dtos;

namespace wfc.referential.API.Endpoints.Sector;

public class PatchSector(IMediator _mediator) : Endpoint<PatchSectorRequest, Guid>
{
    public override void Configure()
    {
        Patch("/api/sectors/{SectorId}");
        AllowAnonymous();

        Summary(s =>
        {
            s.Summary = "Partially update a Sector's properties";
            s.Description = "Updates only the provided fields of the specified sector ID.";
            s.Params["SectorId"] = "Sector ID (GUID) from route";

            s.Response<Guid>(200, "The ID of the updated sector");
            s.Response(400, "If validation fails or the ID is invalid");
            s.Response(404, "Sector not found");
        });

        Options(o => o.WithTags(EndpointGroups.Sectors));
    }

    public override async Task HandleAsync(PatchSectorRequest req, CancellationToken ct)
    {
        var command = req.Adapt<PatchSectorCommand>();
        var updatedId = await _mediator.Send(command, ct);
        await SendAsync(updatedId, cancellation: ct);
    }
}