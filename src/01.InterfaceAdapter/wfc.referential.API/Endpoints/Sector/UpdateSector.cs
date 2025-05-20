using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.Sectors.Commands.UpdateSector;
using wfc.referential.Application.Sectors.Dtos;

namespace wfc.referential.API.Endpoints.Sector;

public class UpdateSector(IMediator _mediator) : Endpoint<UpdateSectorRequest, Guid>
{
    public override void Configure()
    {
        Put("/api/sectors/{SectorId}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Update an existing Sector";
            s.Description = "Updates the sector identified by SectorId with new code, name, country, city, and optional region.";
            s.Params["SectorId"] = "Sector ID (GUID) from route";

            s.Response<Guid>(200, "Returns the ID of the updated sector upon success");
            s.Response(400, "Returned if validation fails (missing fields, invalid ID, etc.)");
            s.Response(404, "Returned if the sector does not exist");
            s.Response(500, "Server error if something unexpected occurs");
        });

        Options(o => o.WithTags(EndpointGroups.Sectors));
    }

    public override async Task HandleAsync(UpdateSectorRequest dto, CancellationToken ct)
    {
        var command = dto.Adapt<UpdateSectorCommand>();
        var sectorId = await _mediator.Send(command, ct);
        await SendAsync(sectorId, cancellation: ct);
    }
}