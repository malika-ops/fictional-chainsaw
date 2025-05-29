using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.Regions.Commands.UpdateRegion;
using wfc.referential.Application.Regions.Dtos;

namespace wfc.referential.API.Endpoints.Region;

public class PutRegion(IMediator _mediator) : Endpoint<UpdateRegionRequest, bool>
{
    public override void Configure()
    {
        Put("api/regions/{RegionId}");
        AllowAnonymous();

        Summary(s =>
        {
            s.Summary = "Fully update a Region's properties";
            s.Description = "Updates all fields (code, name, status, countryId) of the specified Region ID.";
            s.Params["RegionId"] = "Region ID (GUID) from route";

            s.Response<Guid>(200, "The ID of the updated Region");
            s.Response(400, "If validation fails or the ID is invalid");
            s.Response(404, "Region not found");
        });

        Options(o => o.WithTags(EndpointGroups.Regions));
    }

    public override async Task HandleAsync(UpdateRegionRequest req, CancellationToken ct)
    {
        var command = req.Adapt<UpdateRegionCommand>();

        var updatedId = await _mediator.Send(command, ct);

        await SendAsync(updatedId.Value, cancellation: ct);
    }
}
