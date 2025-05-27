using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.Sectors.Commands.DeleteSector;
using wfc.referential.Application.Sectors.Dtos;

namespace wfc.referential.API.Endpoints.Sector;

public class DeleteSectorEndpoint(IMediator _mediator)
    : Endpoint<DeleteSectorRequest, bool>
{
    public override void Configure()
    {
        Delete("/api/sectors/{SectorId}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Delete a sector by GUID";
            s.Description = "Soft-deletes the sector identified by {SectorId}.";
            s.Params["SectorId"] = "GUID of the sector to delete";
            s.Response<bool>(200, "True if deletion succeeded");
            s.Response(400, "Validation / business rule failure");
        });
        Options(o => o.WithTags(EndpointGroups.Sectors));
    }

    public override async Task HandleAsync(DeleteSectorRequest req, CancellationToken ct)
    {
        var command = req.Adapt<DeleteSectorCommand>();
        var result = await _mediator.Send(command, ct);
        await SendAsync(result.Value, cancellation: ct);
    }
}
