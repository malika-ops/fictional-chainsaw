using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.Services.Commands.DeleteService;
using wfc.referential.Application.Services.Dtos;

namespace wfc.referential.API.Endpoints.Service;

public class DeleteService(IMediator _mediator) : Endpoint<DeleteServiceRequest, bool>
{
    public override void Configure()
    {
        Delete("api/services/{ServiceId}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Delete a Service";
        });
        Options(o => o.WithTags(EndpointGroups.Services));
    }

    public override async Task HandleAsync(DeleteServiceRequest req, CancellationToken ct)
    {
        var command = req.Adapt<DeleteServiceCommand>();
        var result = await _mediator.Send(command, ct);
        await SendAsync(result.Value,200, cancellation: ct);
    }
}