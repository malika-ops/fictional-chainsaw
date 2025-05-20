using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.Sectors.Commands.CreateSector;
using wfc.referential.Application.Sectors.Dtos;

namespace wfc.referential.API.Endpoints.Sector;

public class CreateSector(IMediator _mediator) : Endpoint<CreateSectorRequest, Guid>
{
    public override void Configure()
    {
        Post("/api/sectors");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Create a new Sector";
            s.Description = "Creates a sector with code, name, country, city, and optional region. " +
                            "Code must be unique and name/country/city are required.";

            s.Response<Guid>(200, "Returns the ID of the newly created Sector if successful");
            s.Response(400, "Returned if validation fails (missing fields or duplicate code)");
            s.Response(500, "Server error if an unexpected exception occurs");
        });

        Options(o => o.WithTags(EndpointGroups.Sectors));
    }

    public override async Task HandleAsync(CreateSectorRequest dto, CancellationToken ct)
    {
        var command = dto.Adapt<CreateSectorCommand>();
        var result = await _mediator.Send(command, ct);
        await SendAsync(result.Value, cancellation: ct);
    }
}