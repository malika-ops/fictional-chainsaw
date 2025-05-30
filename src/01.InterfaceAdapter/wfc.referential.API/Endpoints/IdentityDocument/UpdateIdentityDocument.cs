using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.IdentityDocuments.Commands.UpdateIdentityDocument;
using wfc.referential.Application.IdentityDocuments.Dtos;

namespace wfc.referential.API.Endpoints.IdentityDocument;

public class UpdateIdentityDocumentEndpoint(IMediator _mediator)
    : Endpoint<UpdateIdentityDocumentRequest, bool>
{
    public override void Configure()
    {
        Put("/api/identitydocuments/{IdentityDocumentId}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Update an existing Identity Document";
            s.Description = "Updates the identity document identified by {IdentityDocumentId} with supplied body fields.";
            s.Params["IdentityDocumentId"] = "Identity Document GUID (from route)";

            s.Response<bool>(200, "Returns true if update succeeded");
            s.Response(400, "Validation or business rule failure");
            s.Response(404, "Identity Document not found");
            s.Response(409, "Conflict with an existing Identity Document");
            s.Response(500, "Unexpected server error");
        });
        Options(o => o.WithTags(EndpointGroups.IdentityDocuments));
    }

    public override async Task HandleAsync(UpdateIdentityDocumentRequest req, CancellationToken ct)
    {
        var cmd = req.Adapt<UpdateIdentityDocumentCommand>();
        var result = await _mediator.Send(cmd, ct);
        await SendAsync(result.Value, cancellation: ct);
    }
}