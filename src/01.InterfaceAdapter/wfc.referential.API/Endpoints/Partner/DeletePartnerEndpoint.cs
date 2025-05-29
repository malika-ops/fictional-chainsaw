using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.Partners.Commands.DeletePartner;
using wfc.referential.Application.Partners.Dtos;

namespace wfc.referential.API.Endpoints.Partner;

public class DeletePartnerEndpoint(IMediator _mediator) : Endpoint<DeletePartnerRequest, bool>
{
    public override void Configure()
    {
        Delete("/api/partners/{PartnerId}");
        AllowAnonymous();

        Summary(s =>
        {
            s.Summary = "Delete a partner by GUID";
            s.Description = "Soft-deletes the partner identified by {PartnerId}.";
            s.Params["PartnerId"] = "GUID of the partner to delete";
            s.Response<bool>(200, "True if deletion succeeded");
            s.Response(400, "Validation / business rule failure");
            s.Response(404, "Partner not found");
            s.Response(409, "Partner has linked support accounts and cannot be deleted");
        });

        Options(o => o.WithTags(EndpointGroups.Partners));
    }

    public override async Task HandleAsync(DeletePartnerRequest req, CancellationToken ct)
    {
        var command = req.Adapt<DeletePartnerCommand>();
        var result = await _mediator.Send(command, ct);
        await SendAsync(result.Value, cancellation: ct);
    }
}