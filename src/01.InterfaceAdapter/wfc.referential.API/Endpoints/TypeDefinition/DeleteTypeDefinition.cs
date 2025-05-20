using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.TypeDefinitions.Commands.DeleteTypeDefinition;
using wfc.referential.Application.TypeDefinitions.Dtos;


namespace wfc.referential.API.Endpoints.TypeDefinition;

public class DeleteTypeDefinition(IMediator _mediator) : Endpoint<DeleteTypeDefinitionRequest, bool>
{
    public override void Configure()
    {
        Delete("/api/typedefinitions/{TypeDefinitionId}");
        AllowAnonymous();

        Summary(s =>
        {
            s.Summary = "Delete a TypeDefinition by GUID";
            s.Description = "Deletes the TypeDefinition identified by {TypeDefinitionId}, as route param.";

            s.Response<bool>(200, "True if deletion succeeded");
            s.Response(400, "If if deletion failed");
        });

        Options(o => o.WithTags(EndpointGroups.TypeDefinitions));
    }

    public override async Task HandleAsync(DeleteTypeDefinitionRequest dto, CancellationToken ct)
    {
        var command = dto.Adapt<DeleteTypeDefinitionCommand>();
        var result = await _mediator.Send(command, ct);
        await SendAsync(result, cancellation: ct);
    }
}
