using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.SupportAccounts.Commands.DeleteSupportAccount;
using wfc.referential.Application.SupportAccounts.Dtos;

namespace wfc.referential.API.Endpoints.SupportAccount;

public class DeleteSupportAccount(IMediator _mediator) : Endpoint<DeleteSupportAccountRequest, bool>
{
    public override void Configure()
    {
        Delete("/api/support-accounts/{SupportAccountId}");
        AllowAnonymous();

        Summary(s =>
        {
            s.Summary = "Delete a Support Account by GUID";
            s.Description = "Deletes the Support Account identified by {SupportAccountId}, as route param.";

            s.Response<bool>(200, "True if deletion succeeded");
            s.Response(400, "If deletion failed due to validation errors");
            s.Response(404, "If support account was not found");
        });

        Options(o => o.WithTags(EndpointGroups.SupportAccounts));
    }

    public override async Task HandleAsync(DeleteSupportAccountRequest dto, CancellationToken ct)
    {
        var command = dto.Adapt<DeleteSupportAccountCommand>();
        var result = await _mediator.Send(command, ct);
        await SendAsync(result, cancellation: ct);
    }
}