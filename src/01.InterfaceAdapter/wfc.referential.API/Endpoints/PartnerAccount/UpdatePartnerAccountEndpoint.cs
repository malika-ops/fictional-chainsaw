using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.PartnerAccounts.Commands.UpdatePartnerAccount;
using wfc.referential.Application.PartnerAccounts.Dtos;

namespace wfc.referential.API.Endpoints.PartnerAccount;

public class UpdatePartnerAccountEndpoint(IMediator _mediator) : Endpoint<UpdatePartnerAccountRequest, bool>
{
    public override void Configure()
    {
        Put("/api/partner-accounts/{PartnerAccountId}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Update an existing Partner Account";
            s.Description = "Updates the partner account identified by {PartnerAccountId} with supplied body fields.";
            s.Params["PartnerAccountId"] = "Partner Account GUID (from route)";
            s.Response<bool>(200, "Returns true if update succeeded");
            s.Response(400, "Validation or business rule failure");
            s.Response(404, "Partner account not found");
            s.Response(409, "Conflict with existing account number/RIB");
            s.Response(500, "Unexpected server error");
        });
        Options(o => o.WithTags(EndpointGroups.PartnerAccounts));
    }

    public override async Task HandleAsync(UpdatePartnerAccountRequest req, CancellationToken ct)
    {
        var cmd = req.Adapt<UpdatePartnerAccountCommand>();
        var result = await _mediator.Send(cmd, ct);
        await SendAsync(result.Value, cancellation: ct);
    }
}