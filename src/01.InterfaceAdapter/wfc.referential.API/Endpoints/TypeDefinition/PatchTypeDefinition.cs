using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.TypeDefinitions.Commands.PatchTypeDefinition;
using wfc.referential.Application.TypeDefinitions.Dtos;

namespace wfc.referential.API.Endpoints.TypeDefinition;

public class PatchTypeDefinition(IMediator _mediator) : Endpoint<PatchTypeDefinitionRequest, Guid>
{
    public override void Configure()
    {
        Patch("/api/typedefinitions/{TypeDefinitionId}");
        AllowAnonymous();

        Summary(s =>
        {
            s.Summary = "Partially update a TypeDefinition's properties";
            s.Description = "Updates only the provided fields of the specified TypeDefinition ID.";
            s.Params["TypeDefinitionId"] = "TypeDefinition ID (GUID) from route";

            s.Response<Guid>(200, "The ID of the updated TypeDefinition");
            s.Response(400, "If validation fails or the ID is invalid");
            s.Response(404, "TypeDefinition not found");
        });

        Options(o => o.WithTags(EndpointGroups.TypeDefinitions));
    }

    public override async Task HandleAsync(PatchTypeDefinitionRequest req, CancellationToken ct)
    {
        var command = req.Adapt<PatchTypeDefinitionCommand>();
        var result = await _mediator.Send(command, ct);
        await SendAsync(result.Value, cancellation: ct);
    }
}