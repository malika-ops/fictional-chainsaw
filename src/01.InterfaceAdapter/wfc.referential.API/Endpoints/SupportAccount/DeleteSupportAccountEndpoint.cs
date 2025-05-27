using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.SupportAccounts.Commands.DeleteSupportAccount;
using wfc.referential.Application.SupportAccounts.Dtos;

namespace wfc.referential.API.Endpoints.SupportAccount;

public class DeleteSupportAccountEndpoint(IMediator _mediator)
    : Endpoint<DeleteSupportAccountRequest, bool>
{
    public override void Configure()
    {
        Delete("/api/support-accounts/{SupportAccountId}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Delete a support account by GUID";
            s.Description = "Soft-deletes the support account identified by {SupportAccountId}.";
            s.Params["SupportAccountId"] = "GUID of the support account to delete";
            s.Response<bool>(200, "True if deletion succeeded");
            s.Response(400, "Validation / business rule failure");
            s.Response(404, "Support account not found");
            s.Response(409, "Support account has linked transactions and cannot be deleted");
        });
        Options(o => o.WithTags(EndpointGroups.SupportAccounts));
    }

    public override async Task HandleAsync(DeleteSupportAccountRequest req, CancellationToken ct)
    {
        var command = req.Adapt<DeleteSupportAccountCommand>();
        var result = await _mediator.Send(command, ct);
        await SendAsync(result.Value, cancellation: ct);
    }
}