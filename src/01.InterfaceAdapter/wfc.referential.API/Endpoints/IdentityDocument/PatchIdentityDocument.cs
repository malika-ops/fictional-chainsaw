using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.IdentityDocuments.Commands.PatchIdentityDocument;
using wfc.referential.Application.IdentityDocuments.Dtos;

namespace wfc.referential.API.Endpoints.IdentityDocument;

public class PatchIdentityDocumentEndpoint(IMediator _mediator)
    : Endpoint<PatchIdentityDocumentRequest, bool>
{
    public override void Configure()
    {
        Patch("/api/identitydocuments/{IdentityDocumentId}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Partially update an Identity Document";
            s.Description =
                "Updates only the supplied fields for the identity document identified by {IdentityDocumentId}.";
            s.Params["IdentityDocumentId"] = "Identity Document GUID from route";

            s.Response<bool>(200, "Returns true if update succeeded");
            s.Response(400, "Validation / business rule failure");
            s.Response(404, "Identity Document not found");
            s.Response(409, "Conflict with an existing Identity Document");
        });
        Options(o => o.WithTags(EndpointGroups.IdentityDocuments));
    }

    public override async Task HandleAsync(PatchIdentityDocumentRequest req, CancellationToken ct)
    {
        var cmd = req.Adapt<PatchIdentityDocumentCommand>();
        var result = await _mediator.Send(cmd, ct);
        await SendAsync(result.Value, cancellation: ct);
    }
}