using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.Agencies.Commands.CreateAgency;
using wfc.referential.Application.Agencies.Dtos;

namespace wfc.referential.API.Endpoints.Agency;

public class CreateAgencyEndpoint(IMediator _mediator) : Endpoint<CreateAgencyRequest, Guid>
{
    public override void Configure()
    {
        Post("/api/agencies");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Create a new Agency";
            s.Description = "Creates an agency with the provided code, label, address, phone, accounting data, geo‑location, etc.";
            s.Response<Guid>(200, "Identifier of the newly created agency");
            s.Response(400, "Validation failed");
            s.Response(500, "Unexpected server error");
            s.Response(409, "Agency code already exists");

        });
        Options(o => o.WithTags(EndpointGroups.Agencies));
    }

    public override async Task HandleAsync(CreateAgencyRequest req, CancellationToken ct)
    {
        var command = req.Adapt<CreateAgencyCommand>();
        var result = await _mediator.Send(command, ct);
        await SendAsync(result.Value, cancellation: ct);
    }
}
