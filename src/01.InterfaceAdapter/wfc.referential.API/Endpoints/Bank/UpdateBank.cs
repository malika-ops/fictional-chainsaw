using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.Banks.Commands.UpdateBank;
using wfc.referential.Application.Banks.Dtos;

namespace wfc.referential.API.Endpoints.Bank;

public class UpdateBank(IMediator _mediator) : Endpoint<UpdateBankRequest, Guid>
{
    public override void Configure()
    {
        Put("/api/banks/{BankId}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Update an existing Bank";
            s.Description = "Updates the bank identified by BankId with new code, name, abbreviation and status.";
            s.Params["BankId"] = "Bank ID (GUID) from route";

            s.Response<Guid>(200, "Returns the ID of the updated bank upon success");
            s.Response(400, "Returned if validation fails (missing fields, invalid ID, etc.)");
            s.Response(404, "Returned if the bank does not exist");
            s.Response(500, "Server error if something unexpected occurs");
        });

        Options(o => o.WithTags("Banks"));
    }

    public override async Task HandleAsync(UpdateBankRequest dto, CancellationToken ct)
    {
        var command = dto.Adapt<UpdateBankCommand>();
        var bankId = await _mediator.Send(command, ct);
        await SendAsync(bankId, cancellation: ct);
    }
}