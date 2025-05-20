using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.TypeDefinitions.Commands.UpdateTypeDefinition;
using wfc.referential.Application.TypeDefinitions.Dtos;

namespace wfc.referential.API.Endpoints.TypeDefinition;

public class UpdateTypeDefinition(IMediator _mediator) : Endpoint<UpdateTypeDefinitionRequest, Guid>
{
    public override void Configure()
    {
        Put("/api/typedefinitions/{TypeDefinitionId}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Update an existing TypeDefinition";
            s.Description = "Updates the TypeDefinition identified by TypeDefinitionId with new libelle and description.";
            s.Params["TypeDefinitionId"] = "TypeDefinition ID (GUID) from route";

            s.Response<Guid>(200, "Returns the ID of the updated TypeDefinition upon success");
            s.Response(400, "Returned if validation fails (missing fields, invalid ID, etc.)");
            s.Response(404, "Returned if the TypeDefinition does not exist");
            s.Response(500, "Server error if something unexpected occurs");
        });

        Options(o => o.WithTags(EndpointGroups.TypeDefinitions));
    }

    public override async Task HandleAsync(UpdateTypeDefinitionRequest dto, CancellationToken ct)
    {
        var command = dto.Adapt<UpdateTypeDefinitionCommand>();
        var typeDefinitionId = await _mediator.Send(command, ct);
        await SendAsync(typeDefinitionId, cancellation: ct);
    }
}