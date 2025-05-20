using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.Banks.Commands.PatchBank;
using wfc.referential.Application.Banks.Dtos;

namespace wfc.referential.API.Endpoints.Bank;

public class PatchBank(IMediator _mediator) : Endpoint<PatchBankRequest, Guid>
{
    public override void Configure()
    {
        Patch("/api/banks/{BankId}");
        AllowAnonymous();

        Summary(s =>
        {
            s.Summary = "Partially update a Bank's properties";
            s.Description = "Updates only the provided fields of the specified bank ID.";
            s.Params["BankId"] = "Bank ID (GUID) from route";

            s.Response<Guid>(200, "The ID of the updated bank");
            s.Response(400, "If validation fails or the ID is invalid");
            s.Response(404, "Bank not found");
        });

        Options(o => o.WithTags("Banks"));
    }

    public override async Task HandleAsync(PatchBankRequest req, CancellationToken ct)
    {
        var command = req.Adapt<PatchBankCommand>();
        var updatedId = await _mediator.Send(command, ct);
        await SendAsync(updatedId, cancellation: ct);
    }
}