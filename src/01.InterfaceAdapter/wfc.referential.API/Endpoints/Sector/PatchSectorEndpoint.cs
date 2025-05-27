using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.Sectors.Commands.PatchSector;
using wfc.referential.Application.Sectors.Dtos;

namespace wfc.referential.API.Endpoints.Sector;

public class PatchSectorEndpoint(IMediator _mediator)
    : Endpoint<PatchSectorRequest, bool>
{
    public override void Configure()
    {
        Patch("/api/sectors/{SectorId}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Partially update a Sector";
            s.Description =
                "Updates only the supplied fields for the sector identified by {SectorId}.";
            s.Params["SectorId"] = "Sector GUID from route";

            s.Response<bool>(200, "Returns true if update succeeded");
            s.Response(400, "Validation / business rule failure");
            s.Response(404, "Sector not found");
            s.Response(409, "Conflict with an existing Sector");
        });
        Options(o => o.WithTags(EndpointGroups.Sectors));
    }

    public override async Task HandleAsync(PatchSectorRequest req, CancellationToken ct)
    {
        var cmd = req.Adapt<PatchSectorCommand>();
        var result = await _mediator.Send(cmd, ct);
        await SendAsync(result.Value, cancellation: ct);
    }
}