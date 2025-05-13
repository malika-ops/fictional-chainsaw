using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.MonetaryZones.Commands.CreateMonetaryZone;
using wfc.referential.Application.MonetaryZones.Dtos;


namespace wfc.referential.API.Endpoints.MonetaryZoneEndpoints;

public class CreateMonetaryZone(IMediator _mediator) : 
    Endpoint<CreateMonetaryZoneRequest, Guid>
{
    public override void Configure()
    {
        Post("/api/monetaryZones");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Create a new MonetaryZone";
            s.Description = "Creates a monetary zone with code, name, and description. " +
                            "Code must be unique and name/description are required.";

            s.Response<Guid>(200, "Returns the ID of the newly created MonetaryZone if successful");
            s.Response(400, "Returned if validation fails (missing fields or duplicate code)");
            s.Response(500, "Server error if an unexpected exception occurs");
        });


        Options(o => o.WithTags(EndpointGroups.MonetaryZones));
    }

    public override async Task HandleAsync(CreateMonetaryZoneRequest dto, CancellationToken ct)
    {
        var command = dto.Adapt<CreateMonetaryZoneCommand>();
        var result = await _mediator.Send(command, ct);
        await SendAsync(result.Value, cancellation: ct);
    }
}