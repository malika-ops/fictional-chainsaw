using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.PartnerAccounts.Commands.DeletePartnerAccount;
using wfc.referential.Application.PartnerAccounts.Dtos;

namespace wfc.referential.API.Endpoints.PartnerAccount;

public class DeletePartnerAccountEndpoint(IMediator _mediator) : Endpoint<DeletePartnerAccountRequest, bool>
{
    public override void Configure()
    {
        Delete("/api/partner-accounts/{PartnerAccountId}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Delete a partner account by GUID";
            s.Description = "Soft-deletes the partner account identified by {PartnerAccountId}.";
            s.Params["PartnerAccountId"] = "GUID of the partner account to delete";
            s.Response<bool>(200, "True if deletion succeeded");
            s.Response(400, "Validation / business rule failure");
            s.Response(404, "Partner account not found");
            s.Response(409, "Partner account has linked transactions and cannot be deleted");
        });
        Options(o => o.WithTags(EndpointGroups.PartnerAccounts));
    }

    public override async Task HandleAsync(DeletePartnerAccountRequest req, CancellationToken ct)
    {
        var command = req.Adapt<DeletePartnerAccountCommand>();
        var result = await _mediator.Send(command, ct);
        await SendAsync(result.Value, cancellation: ct);
    }
}