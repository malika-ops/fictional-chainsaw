using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.Sectors.Commands.DeleteSector;
using wfc.referential.Application.Sectors.Dtos;

namespace wfc.referential.API.Endpoints.Sector;

public class DeleteSector(IMediator _mediator) : Endpoint<DeleteSectorRequest, bool>
{
    public override void Configure()
    {
        Delete("/api/sectors/{SectorId}");
        AllowAnonymous();

        Summary(s =>
        {
            s.Summary = "Delete a Sector by GUID";
            s.Description = "Deletes the Sector identified by {SectorId}, as route param. Will fail if sector has linked agencies.";

            s.Response<bool>(200, "True if deletion succeeded");
            s.Response(400, "If deletion failed due to validation errors");
            s.Response(404, "If sector was not found");
            s.Response(409, "If sector has linked agencies and cannot be deleted");
        });

        Options(o => o.WithTags(EndpointGroups.Sectors));
    }

    public override async Task HandleAsync(DeleteSectorRequest dto, CancellationToken ct)
    {
        var command = dto.Adapt<DeleteSectorCommand>();
        var result = await _mediator.Send(command, ct);
        await SendAsync(result, cancellation: ct);
    }
}