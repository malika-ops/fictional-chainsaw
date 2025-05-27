using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.Sectors.Commands.CreateSector;
using wfc.referential.Application.Sectors.Dtos;

namespace wfc.referential.API.Endpoints.Sector;

public class CreateSectorEndpoint(IMediator _mediator) : Endpoint<CreateSectorRequest, Guid>
{
    public override void Configure()
    {
        Post("/api/sectors");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Create a new Sector";
            s.Description = "Creates a sector with the provided code, name, and city association.";
            s.Response<Guid>(200, "Identifier of the newly created sector");
            s.Response(400, "Validation failed");
            s.Response(500, "Unexpected server error");
        });
        Options(o => o.WithTags(EndpointGroups.Sectors));
    }

    public override async Task HandleAsync(CreateSectorRequest req, CancellationToken ct)
    {
        var command = req.Adapt<CreateSectorCommand>();
        var result = await _mediator.Send(command, ct);
        await SendAsync(result.Value, cancellation: ct);
    }
}
