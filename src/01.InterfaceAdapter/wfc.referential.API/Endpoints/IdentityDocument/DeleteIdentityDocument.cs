using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.IdentityDocuments.Commands.DeleteIdentityDocument;
using wfc.referential.Application.IdentityDocuments.Dtos;

namespace wfc.referential.API.Endpoints.IdentityDocument;

public class DeleteIdentityDocument(IMediator _mediator) : Endpoint<DeleteIdentityDocumentRequest, bool>
{
    public override void Configure()
    {
        Delete("api/identitydocuments/{IdentityDocumentId}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Delete a IdentityDocument";
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