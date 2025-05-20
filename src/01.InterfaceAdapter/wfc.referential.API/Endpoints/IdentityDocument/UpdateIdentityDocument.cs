using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.IdentityDocuments.Commands.UpdateIdentityDocument;
using wfc.referential.Application.IdentityDocuments.Dtos;

namespace wfc.referential.API.Endpoints.IdentityDocument;

public class PutIdentityDocument(IMediator _mediator) : Endpoint<UpdateIdentityDocumentRequest, Guid>
{
    public override void Configure()
    {
        Put("api/identitydocuments/{IdentityDocumentId}");
        AllowAnonymous();

        Summary(s =>
        {
            s.Summary = "Fully update a IdentityDocument's properties";
            s.Description = "Updates all fields (code, name, status, countryId) of the specified IdentityDocument ID.";
            s.Params["IdentityDocumentId"] = "IdentityDocument ID (GUID) from route";

            s.Response<Guid>(200, "The ID of the updated IdentityDocument");
            s.Response(400, "If validation fails or the ID is invalid");
            s.Response(404, "IdentityDocument not found");
        });

        Options(o => o.WithTags(EndpointGroups.IdentityDocuments));
    }

    public override async Task HandleAsync(UpdateIdentityDocumentRequest req, CancellationToken ct)
    {
        var command = req.Adapt<UpdateIdentityDocumentCommand>();

        var updatedId = await _mediator.Send(command, ct);

        await SendAsync(updatedId.Value, cancellation: ct);
    }
}
