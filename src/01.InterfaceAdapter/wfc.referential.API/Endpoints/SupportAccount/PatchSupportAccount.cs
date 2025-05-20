using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.SupportAccounts.Commands.PatchSupportAccount;
using wfc.referential.Application.SupportAccounts.Dtos;

namespace wfc.referential.API.Endpoints.SupportAccount;

public class PatchSupportAccount(IMediator _mediator) : Endpoint<PatchSupportAccountRequest, Guid>
{
    public override void Configure()
    {
        Patch("/api/support-accounts/{SupportAccountId}");
        AllowAnonymous();

        Summary(s =>
        {
            s.Summary = "Partially update a Support Account's properties";
            s.Description = "Updates only the provided fields of the specified support account ID.";
            s.Params["SupportAccountId"] = "Support Account ID (GUID) from route";

            s.Response<Guid>(200, "The ID of the updated support account");
            s.Response(400, "If validation fails or the ID is invalid");
            s.Response(404, "Support account not found");
        });

        Options(o => o.WithTags(EndpointGroups.SupportAccounts));
    }

    public override async Task HandleAsync(PatchSupportAccountRequest req, CancellationToken ct)
    {
        var command = req.Adapt<PatchSupportAccountCommand>();
        var updatedId = await _mediator.Send(command, ct);
        await SendAsync(updatedId, cancellation: ct);
    }
}