using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.Banks.Commands.UpdateBank;
using wfc.referential.Application.Banks.Dtos;

namespace wfc.referential.API.Endpoints.Bank;

public class UpdateBankEndpoint(IMediator _mediator)
    : Endpoint<UpdateBankRequest, bool>
{
    public override void Configure()
    {
        Put("/api/banks/{BankId}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Update an existing Bank";
            s.Description = "Updates the bank identified by {BankId} with supplied body fields.";
            s.Params["BankId"] = "Bank GUID (from route)";

            s.Response<bool>(200, "Returns true if update succeeded");
            s.Response(400, "Validation or business rule failure");
            s.Response(404, "Bank not found");
            s.Response(409, "Conflict with an existing Bank");
            s.Response(500, "Unexpected server error");
        });
        Options(o => o.WithTags(EndpointGroups.Banks));
    }

    public override async Task HandleAsync(UpdateBankRequest req, CancellationToken ct)
    {
        var cmd = req.Adapt<UpdateBankCommand>();
        var result = await _mediator.Send(cmd, ct);
        await SendAsync(result.Value, cancellation: ct);
    }
}