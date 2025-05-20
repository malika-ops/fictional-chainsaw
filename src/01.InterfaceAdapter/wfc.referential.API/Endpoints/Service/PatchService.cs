using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.Services.Commands.PatchService;
using wfc.referential.Application.Services.Dtos;


namespace wfc.referential.API.Endpoints.Service;

public class PatchService(IMediator _mediator) : Endpoint<PatchServiceRequest, Guid>
{
    public override void Configure()
    {
        Patch("api/services/{ServiceId}");
        AllowAnonymous();

        Summary(s =>
        {
            s.Summary = "Partially update a Service's properties";
            s.Description = "Updates only the provided fields (code, name, status) of the specified Service ID.";
            s.Params["ServiceId"] = "Service ID (GUID) from route";

            s.Response<Guid>(200, "The ID of the updated Service");
            s.Response(400, "If validation fails or the ID is invalid");
            s.Response(404, "Service not found");
        });
        Options(o => o.WithTags(EndpointGroups.Services));
    }

    public override async Task HandleAsync(PatchServiceRequest req, CancellationToken ct)
    {
        var command = req.Adapt<PatchServiceCommand>();
        var result = await _mediator.Send(command, ct);
        await SendAsync(result.Value, cancellation: ct);
    }
}