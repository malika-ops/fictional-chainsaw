using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.Banks.Commands.DeleteBank;
using wfc.referential.Application.Banks.Dtos;

namespace wfc.referential.API.Endpoints.Bank;

public class DeleteBank(IMediator _mediator) : Endpoint<DeleteBankRequest, bool>
{
    public override void Configure()
    {
        Delete("/api/banks/{BankId}");
        AllowAnonymous();

        Summary(s =>
        {
            s.Summary = "Delete a Bank by GUID";
            s.Description = "Deletes the Bank identified by {BankId}, as route param. Will fail if bank has linked accounts.";

            s.Response<bool>(200, "True if deletion succeeded");
            s.Response(400, "If deletion failed due to validation errors");
            s.Response(404, "If bank was not found");
            s.Response(409, "If bank has linked accounts and cannot be deleted");
        });

        Options(o => o.WithTags("Banks"));
    }

    public override async Task HandleAsync(DeleteBankRequest dto, CancellationToken ct)
    {
        var command = dto.Adapt<DeleteBankCommand>();
        var result = await _mediator.Send(command, ct);
        await SendAsync(result, cancellation: ct);
    }
}