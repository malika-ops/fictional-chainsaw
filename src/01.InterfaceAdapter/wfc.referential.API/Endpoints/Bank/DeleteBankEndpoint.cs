using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.Banks.Commands.DeleteBank;
using wfc.referential.Application.Banks.Dtos;

namespace wfc.referential.API.Endpoints.Bank;

public class DeleteBankEndpoint(IMediator _mediator)
    : Endpoint<DeleteBankRequest, bool>
{
    public override void Configure()
    {
        Delete("/api/banks/{BankId}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Delete a bank by GUID";
            s.Description = "Soft-deletes the bank identified by {BankId}.";
            s.Params["BankId"] = "GUID of the bank to delete";
            s.Response<bool>(200, "True if deletion succeeded");
            s.Response(400, "Validation / business rule failure");
            s.Response(404, "Bank not found");
            s.Response(409, "Bank has linked accounts and cannot be deleted");
        });
        Options(o => o.WithTags(EndpointGroups.Banks));
    }

    public override async Task HandleAsync(DeleteBankRequest req, CancellationToken ct)
    {
        var command = req.Adapt<DeleteBankCommand>();
        var result = await _mediator.Send(command, ct);
        await SendAsync(result.Value, cancellation: ct);
    }
}