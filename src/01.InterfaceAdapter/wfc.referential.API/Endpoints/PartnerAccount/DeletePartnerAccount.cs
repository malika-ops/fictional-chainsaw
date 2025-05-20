using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.PartnerAccounts.Commands.DeletePartnerAccount;
using wfc.referential.Application.PartnerAccounts.Dtos;

namespace wfc.referential.API.Endpoints.PartnerAccount;

public class DeletePartnerAccount(IMediator _mediator) : Endpoint<DeletePartnerAccountRequest, bool>
{
    public override void Configure()
    {
        Delete("/api/partner-accounts/{PartnerAccountId}");
        AllowAnonymous();

        Summary(s =>
        {
            s.Summary = "Delete a Partner Account by GUID";
            s.Description = "Deletes the Partner Account identified by {PartnerAccountId}, as route param.";

            s.Response<bool>(200, "True if deletion succeeded");
            s.Response(400, "If deletion failed due to validation errors");
            s.Response(404, "If partner account was not found");
        });

        Options(o => o.WithTags("PartnerAccounts"));
    }

    public override async Task HandleAsync(DeletePartnerAccountRequest dto, CancellationToken ct)
    {
        var command = dto.Adapt<DeletePartnerAccountCommand>();
        var result = await _mediator.Send(command, ct);
        await SendAsync(result, cancellation: ct);
    }
}