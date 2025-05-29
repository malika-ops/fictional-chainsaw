using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.Regions.Commands.PatchRegion;
using wfc.referential.Application.Regions.Dtos;


namespace wfc.referential.API.Endpoints.Region;

public class PatchRegion(IMediator _mediator) : Endpoint<PatchRegionRequest, bool>
{
    public override void Configure()
    {
        Patch("api/regions/{RegionId}"); 
        AllowAnonymous();

        Summary(s =>
        {
            s.Summary = "Partially update a Region's properties";
            s.Description = "Updates only the provided fields (code, name, statu or countryId) of the specified Region ID.";
            s.Params["RegionId"] = "Region ID (GUID) from route";

            s.Response<Guid>(200, "The ID of the updated Region");
            s.Response(400, "If validation fails or the ID is invalid");
            s.Response(404, "Region not found");
        });

        Options(o => o.WithTags(EndpointGroups.Regions));
    }

    public override async Task HandleAsync(PatchRegionRequest req, CancellationToken ct)
    {
        var command = req.Adapt<PatchRegionCommand>();

        var updatedId = await _mediator.Send(command, ct);

        await SendAsync(updatedId.Value, cancellation: ct);
    }
}
