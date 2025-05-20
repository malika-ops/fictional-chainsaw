using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.SupportAccounts.Commands.UpdateSupportAccount;
using wfc.referential.Application.SupportAccounts.Dtos;

namespace wfc.referential.API.Endpoints.SupportAccount;

public class UpdateSupportAccount(IMediator _mediator) : Endpoint<UpdateSupportAccountRequest, Guid>
{
    public override void Configure()
    {
        Put("/api/support-accounts/{SupportAccountId}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Update an existing Support Account";
            s.Description = "Updates the support account identified by SupportAccountId with new account details.";
            s.Params["SupportAccountId"] = "Support Account ID (GUID) from route";

            s.Response<Guid>(200, "Returns the ID of the updated support account upon success");
            s.Response(400, "Returned if validation fails (missing fields, invalid ID, etc.)");
            s.Response(404, "Returned if the support account does not exist");
            s.Response(500, "Server error if something unexpected occurs");
        });

        Options(o => o.WithTags(EndpointGroups.SupportAccounts));
    }

    public override async Task HandleAsync(UpdateSupportAccountRequest dto, CancellationToken ct)
    {
        var command = dto.Adapt<UpdateSupportAccountCommand>();
        var supportAccountId = await _mediator.Send(command, ct);
        await SendAsync(supportAccountId, cancellation: ct);
    }
}