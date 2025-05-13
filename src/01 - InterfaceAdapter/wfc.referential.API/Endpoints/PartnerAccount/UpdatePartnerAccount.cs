using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.PartnerAccounts.Commands.UpdatePartnerAccount;
using wfc.referential.Application.PartnerAccounts.Dtos;

namespace wfc.referential.API.Endpoints.PartnerAccount;

public class UpdatePartnerAccount(IMediator _mediator) : Endpoint<UpdatePartnerAccountRequest, Guid>
{
    public override void Configure()
    {
        Put("/api/partner-accounts/{PartnerAccountId}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Update an existing Partner Account";
            s.Description = "Updates the partner account identified by PartnerAccountId with new account details.";
            s.Params["PartnerAccountId"] = "Partner Account ID (GUID) from route";

            s.Response<Guid>(200, "Returns the ID of the updated partner account upon success");
            s.Response(400, "Returned if validation fails (missing fields, invalid ID, etc.)");
            s.Response(404, "Returned if the partner account does not exist");
            s.Response(500, "Server error if something unexpected occurs");
        });

        Options(o => o.WithTags("PartnerAccounts"));
    }

    public override async Task HandleAsync(UpdatePartnerAccountRequest dto, CancellationToken ct)
    {
        var command = dto.Adapt<UpdatePartnerAccountCommand>();
        var partnerAccountId = await _mediator.Send(command, ct);
        await SendAsync(partnerAccountId, cancellation: ct);
    }
}