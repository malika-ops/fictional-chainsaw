using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.PartnerAccounts.Commands.UpdateBalance;
using wfc.referential.Application.PartnerAccounts.Dtos;

namespace wfc.referential.API.Endpoints.PartnerAccount;

public class UpdateBalance(IMediator _mediator) : Endpoint<UpdateBalanceRequest, Guid>
{
    public override void Configure()
    {
        Patch("/api/partner-accounts/{PartnerAccountId}/balance");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Update a Partner Account's balance";
            s.Description = "Updates only the balance of the specified partner account ID.";
            s.Params["PartnerAccountId"] = "Partner Account ID (GUID) from route";

            s.Response<Guid>(200, "The ID of the updated partner account");
            s.Response(400, "If validation fails or the ID is invalid");
            s.Response(404, "Partner account not found");
        });

        Options(o => o.WithTags("PartnerAccounts"));
    }

    public override async Task HandleAsync(UpdateBalanceRequest req, CancellationToken ct)
    {
        var command = req.Adapt<UpdateBalanceCommand>();
        var updatedId = await _mediator.Send(command, ct);
        await SendAsync(updatedId, cancellation: ct);
    }
}
