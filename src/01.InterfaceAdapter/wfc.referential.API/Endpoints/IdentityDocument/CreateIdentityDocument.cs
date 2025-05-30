using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.IdentityDocuments.Commands.CreateIdentityDocument;
using wfc.referential.Application.IdentityDocuments.Dtos;

namespace wfc.referential.API.Endpoints.IdentityDocument;

public class CreateIdentityDocumentEndpoint(IMediator _mediator) : Endpoint<CreateIdentityDocumentRequest, Guid>
{
    public override void Configure()
    {
        Post("/api/identitydocuments");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Create a new Identity Document";
            s.Description = "Creates an identity document with the provided code, name, and description.";
            s.Response<Guid>(200, "Identifier of the newly created identity document");
            s.Response(400, "Validation failed");
            s.Response(409, "Conflict with an existing Identity Document");
            s.Response(500, "Unexpected server error");
        });
        Options(o => o.WithTags(EndpointGroups.IdentityDocuments));
    }

    public override async Task HandleAsync(CreateIdentityDocumentRequest req, CancellationToken ct)
    {
        var command = req.Adapt<CreateIdentityDocumentCommand>();
        var result = await _mediator.Send(command, ct);
        await SendAsync(result.Value, cancellation: ct);
    }
}