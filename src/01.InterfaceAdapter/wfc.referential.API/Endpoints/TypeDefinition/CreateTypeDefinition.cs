using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.TypeDefinitions.Commands.CreateTypeDefinition;
using wfc.referential.Application.TypeDefinitions.Dtos;


namespace wfc.referential.API.Endpoints.TypeDefinition;

public class CreateTypeDefinition(IMediator _mediator) : Endpoint<CreateTypeDefinitionRequest, Guid>
{
    public override void Configure()
    {
        Post("/api/typedefinitions");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Create a new typedefinition";
            s.Description = "Creates a type definition with libelle and desscription. ";
            s.Response<Guid>(200, "Returns the ID of the newly created MonetaryZone if successful");
            s.Response(400, "Returned if validation fails (missing fields or duplicate code)");
            s.Response(500, "Server error if an unexpected exception occurs");
        });


        Options(o => o.WithTags(EndpointGroups.TypeDefinitions));
    }

    public override async Task HandleAsync(CreateTypeDefinitionRequest dto, CancellationToken ct)
    {
        var command = dto.Adapt<CreateTypeDefinitionCommand>();
        var result = await _mediator.Send(command, ct);
        await SendAsync(result.Value, cancellation: ct);
    }
}
