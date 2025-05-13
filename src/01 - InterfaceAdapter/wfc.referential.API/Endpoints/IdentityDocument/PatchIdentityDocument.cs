using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.IdentityDocuments.Commands.PatchIdentityDocument;
using wfc.referential.Application.IdentityDocuments.Dtos;


namespace wfc.referential.API.Endpoints.IdentityDocument;

public class PatchIdentityDocument(IMediator _mediator) : Endpoint<PatchIdentityDocumentRequest, Guid>
{
    public override void Configure()
    {
        Patch("api/identitydocuments/{IdentityDocumentId}"); 
        AllowAnonymous();

        Summary(s =>
        {
            s.Summary = "Partially update a IdentityDocument's properties";
            s.Description = "Updates only the provided fields (code, name, statu or countryId) of the specified IdentityDocument ID.";
            s.Params["IdentityDocumentId"] = "IdentityDocument ID (GUID) from route";

            s.Response<Guid>(200, "The ID of the updated IdentityDocument");
            s.Response(400, "If validation fails or the ID is invalid");
            s.Response(404, "IdentityDocument not found");
        });

        Options(o => o.WithTags(EndpointGroups.IdentityDocuments));
    }

    public override async Task HandleAsync(PatchIdentityDocumentRequest req, CancellationToken ct)
    {
        var command = req.Adapt<PatchIdentityDocumentCommand>();

        var updatedId = await _mediator.Send(command, ct);

        await SendAsync(updatedId.Value, cancellation: ct);
    }
}
