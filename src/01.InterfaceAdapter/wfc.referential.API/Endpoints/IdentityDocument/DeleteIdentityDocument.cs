using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.IdentityDocuments.Commands.DeleteIdentityDocument;
using wfc.referential.Application.IdentityDocuments.Dtos;

namespace wfc.referential.API.Endpoints.IdentityDocument;

public class DeleteIdentityDocumentEndpoint(IMediator _mediator)
    : Endpoint<DeleteIdentityDocumentRequest, bool>
{
    public override void Configure()
    {
        Delete("/api/identitydocuments/{IdentityDocumentId}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Delete an identity document by GUID";
            s.Description = "Soft-deletes the identity document identified by {IdentityDocumentId}.";
            s.Params["IdentityDocumentId"] = "GUID of the identity document to delete";
            s.Response<bool>(200, "True if deletion succeeded");
            s.Response(400, "Validation / business rule failure");
            s.Response(404, "Identity Document not found");
        });
        Options(o => o.WithTags(EndpointGroups.IdentityDocuments));
    }

    public override async Task HandleAsync(DeleteIdentityDocumentRequest req, CancellationToken ct)
    {
        var command = req.Adapt<DeleteIdentityDocumentCommand>();
        var result = await _mediator.Send(command, ct);
        await SendAsync(result.Value, cancellation: ct);
    }
}