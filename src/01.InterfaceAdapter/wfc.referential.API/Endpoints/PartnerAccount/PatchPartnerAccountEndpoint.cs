using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.PartnerAccounts.Commands.PatchPartnerAccount;
using wfc.referential.Application.PartnerAccounts.Dtos;

namespace wfc.referential.API.Endpoints.PartnerAccount;

public class PatchPartnerAccountEndpoint(IMediator _mediator) : Endpoint<PatchPartnerAccountRequest, bool>
{
    public override void Configure()
    {
        Patch("/api/partner-accounts/{PartnerAccountId}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Partially update a Partner Account";
            s.Description = "Updates only the supplied fields for the partner account identified by {PartnerAccountId}.";
            s.Params["PartnerAccountId"] = "Partner Account GUID from route";
            s.Response<bool>(200, "Returns true if update succeeded");
            s.Response(400, "Validation / business rule failure");
            s.Response(404, "Partner account not found");
            s.Response(409, "Conflict with existing account number/RIB");
        });
        Options(o => o.WithTags(EndpointGroups.PartnerAccounts));
    }

    public override async Task HandleAsync(PatchPartnerAccountRequest req, CancellationToken ct)
    {
        var cmd = req.Adapt<PatchPartnerAccountCommand>();
        var result = await _mediator.Send(cmd, ct);
        await SendAsync(result.Value, cancellation: ct);
    }
}