using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.SupportAccounts.Commands.UpdateBalance;
using wfc.referential.Application.SupportAccounts.Dtos;

namespace wfc.referential.API.Endpoints.SupportAccount;

public class UpdateBalanceEndpoint(IMediator _mediator)
    : Endpoint<UpdateSupportAccountBalanceRequest, bool>
{
    public override void Configure()
    {
        Patch("/api/support-accounts/{SupportAccountId}/balance");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Update a Support Account's balance";
            s.Description = "Updates only the balance of the support account identified by {SupportAccountId}.";
            s.Params["SupportAccountId"] = "Support Account GUID from route";

            s.Response<bool>(200, "Returns true if balance update succeeded");
            s.Response(400, "Validation / business rule failure");
            s.Response(404, "Support account not found");
        });
        Options(o => o.WithTags(EndpointGroups.SupportAccounts));
    }

    public override async Task HandleAsync(UpdateSupportAccountBalanceRequest req, CancellationToken ct)
    {
        var cmd = req.Adapt<UpdateSupportAccountBalanceCommand>();
        var result = await _mediator.Send(cmd, ct);
        await SendAsync(result.Value, cancellation: ct);
    }
}