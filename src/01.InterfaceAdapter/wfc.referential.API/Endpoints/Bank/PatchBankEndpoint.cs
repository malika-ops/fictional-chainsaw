using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.Banks.Commands.PatchBank;
using wfc.referential.Application.Banks.Dtos;

namespace wfc.referential.API.Endpoints.Bank;

public class PatchBankEndpoint(IMediator _mediator)
    : Endpoint<PatchBankRequest, bool>
{
    public override void Configure()
    {
        Patch("/api/banks/{BankId}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Partially update a Bank";
            s.Description =
                "Updates only the supplied fields for the bank identified by {BankId}.";
            s.Params["BankId"] = "Bank GUID from route";

            s.Response<bool>(200, "Returns true if update succeeded");
            s.Response(400, "Validation / business rule failure");
            s.Response(404, "Bank not found");
            s.Response(409, "Conflict with an existing Bank");
        });
        Options(o => o.WithTags(EndpointGroups.Banks));
    }

    public override async Task HandleAsync(PatchBankRequest req, CancellationToken ct)
    {
        var cmd = req.Adapt<PatchBankCommand>();
        var result = await _mediator.Send(cmd, ct);
        await SendAsync(result.Value, cancellation: ct);
    }
}