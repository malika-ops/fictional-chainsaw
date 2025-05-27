using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.SupportAccounts.Commands.PatchSupportAccount;
using wfc.referential.Application.SupportAccounts.Dtos;

namespace wfc.referential.API.Endpoints.SupportAccount;

public class PatchSupportAccountEndpoint(IMediator _mediator)
    : Endpoint<PatchSupportAccountRequest, bool>
{
    public override void Configure()
    {
        Patch("/api/support-accounts/{SupportAccountId}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Partially update a Support Account";
            s.Description =
                "Updates only the supplied fields for the support account identified by {SupportAccountId}.";
            s.Params["SupportAccountId"] = "Support Account GUID from route";

            s.Response<bool>(200, "Returns true if update succeeded");
            s.Response(400, "Validation / business rule failure");
            s.Response(404, "Support account not found");
            s.Response(409, "Conflict with an existing Support Account");
        });
        Options(o => o.WithTags(EndpointGroups.SupportAccounts));
    }

    public override async Task HandleAsync(PatchSupportAccountRequest req, CancellationToken ct)
    {
        var cmd = req.Adapt<PatchSupportAccountCommand>();
        var result = await _mediator.Send(cmd, ct);
        await SendAsync(result.Value, cancellation: ct);
    }
}