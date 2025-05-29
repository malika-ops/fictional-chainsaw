using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.PartnerAccounts.Commands.UpdateBalance;
using wfc.referential.Application.PartnerAccounts.Dtos;

namespace wfc.referential.API.Endpoints.PartnerAccount;

public class UpdateBalanceEndpoint(IMediator _mediator) : Endpoint<UpdateBalanceRequest, bool>
{
    public override void Configure()
    {
        Patch("/api/partner-accounts/{PartnerAccountId}/balance");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Update a Partner Account's balance";
            s.Description = "Updates only the balance of the partner account identified by {PartnerAccountId}.";
            s.Params["PartnerAccountId"] = "Partner Account GUID from route";
            s.Response<bool>(200, "Returns true if balance update succeeded");
            s.Response(400, "Validation failure or negative balance");
            s.Response(404, "Partner account not found");
        });
        Options(o => o.WithTags(EndpointGroups.PartnerAccounts));
    }

    public override async Task HandleAsync(UpdateBalanceRequest req, CancellationToken ct)
    {
        var command = req.Adapt<UpdateBalanceCommand>();
        var result = await _mediator.Send(command, ct);
        await SendAsync(result.Value, cancellation: ct);
    }
}