using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.PartnerAccounts.Commands.PatchPartnerAccount;
using wfc.referential.Application.PartnerAccounts.Dtos;

namespace wfc.referential.API.Endpoints.PartnerAccount;

public class PatchPartnerAccount(IMediator _mediator) : Endpoint<PatchPartnerAccountRequest, Guid>
{
    public override void Configure()
    {
        Patch("/api/partner-accounts/{PartnerAccountId}");
        AllowAnonymous();

        Summary(s =>
        {
            s.Summary = "Partially update a Partner Account's properties";
            s.Description = "Updates only the provided fields of the specified partner account ID.";
            s.Params["PartnerAccountId"] = "Partner Account ID (GUID) from route";

            s.Response<Guid>(200, "The ID of the updated partner account");
            s.Response(400, "If validation fails or the ID is invalid");
            s.Response(404, "Partner account not found");
        });

        Options(o => o.WithTags("PartnerAccounts"));
    }

    public override async Task HandleAsync(PatchPartnerAccountRequest req, CancellationToken ct)
    {
        var command = req.Adapt<PatchPartnerAccountCommand>();
        var updatedId = await _mediator.Send(command, ct);
        await SendAsync(updatedId, cancellation: ct);
    }
}