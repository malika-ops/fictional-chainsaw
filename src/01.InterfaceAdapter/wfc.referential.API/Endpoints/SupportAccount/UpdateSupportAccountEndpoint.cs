using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.SupportAccounts.Commands.UpdateSupportAccount;
using wfc.referential.Application.SupportAccounts.Dtos;

namespace wfc.referential.API.Endpoints.SupportAccount;

public class UpdateSupportAccountEndpoint(IMediator _mediator)
    : Endpoint<UpdateSupportAccountRequest, bool>
{
    public override void Configure()
    {
        Put("/api/support-accounts/{SupportAccountId}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Update an existing Support Account";
            s.Description = "Updates the support account identified by {SupportAccountId} with supplied body fields.";
            s.Params["SupportAccountId"] = "Support Account GUID (from route)";

            s.Response<bool>(200, "Returns true if update succeeded");
            s.Response(400, "Validation or business rule failure");
            s.Response(404, "Support account not found");
            s.Response(409, "Conflict with an existing Support Account");
            s.Response(500, "Unexpected server error");
        });
        Options(o => o.WithTags(EndpointGroups.SupportAccounts));
    }

    public override async Task HandleAsync(UpdateSupportAccountRequest req, CancellationToken ct)
    {
        var cmd = req.Adapt<UpdateSupportAccountCommand>();
        var result = await _mediator.Send(cmd, ct);
        await SendAsync(result.Value, cancellation: ct);
    }
}