using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.Regions.Commands.DeleteRegion;
using wfc.referential.Application.Regions.Dtos;

namespace wfc.referential.API.Endpoints.Region;

public class DeleteRegion(IMediator _mediator) : Endpoint<DeleteRegionRequest, bool>
{
    public override void Configure()
    {
        Delete("api/regions/{RegionId}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Delete a region";
        });
        Options(o => o.WithTags(EndpointGroups.Regions));
    }

    public override async Task HandleAsync(DeleteRegionRequest req, CancellationToken ct)
    {
        var command = req.Adapt<DeleteRegionCommand>();
        var result = await _mediator.Send(command, ct);
        await SendAsync(result.Value, cancellation: ct);
    }
}