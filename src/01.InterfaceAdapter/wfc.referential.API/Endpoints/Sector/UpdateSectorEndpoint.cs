using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.Sectors.Commands.UpdateSector;
using wfc.referential.Application.Sectors.Dtos;

namespace wfc.referential.API.Endpoints.Sector;

public class UpdateSectorEndpoint(IMediator _mediator)
    : Endpoint<UpdateSectorRequest, bool>
{
    public override void Configure()
    {
        Put("/api/sectors/{SectorId}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Update an existing Sector";
            s.Description = "Updates the sector identified by {SectorId} with supplied body fields.";
            s.Params["SectorId"] = "Sector GUID (from route)";

            s.Response<bool>(200, "Returns true if update succeeded");
            s.Response(400, "Validation or business rule failure");
            s.Response(500, "Unexpected server error");
            s.Response(409, "Conflict with an existing Sector");
        });
        Options(o => o.WithTags(EndpointGroups.Sectors));
    }

    public override async Task HandleAsync(UpdateSectorRequest req, CancellationToken ct)
    {
        var cmd = req.Adapt<UpdateSectorCommand>();
        var result = await _mediator.Send(cmd, ct);
        await SendAsync(result.Value, cancellation: ct);
    }
}