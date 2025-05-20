using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.Banks.Commands.CreateBank;
using wfc.referential.Application.Banks.Dtos;

namespace wfc.referential.API.Endpoints.Bank;

public class CreateBank(IMediator _mediator) : Endpoint<CreateBankRequest, Guid>
{
    public override void Configure()
    {
        Post("/api/banks");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Create a new Bank";
            s.Description = "Creates a bank with code, name, and abbreviation. Code must be unique.";

            s.Response<Guid>(200, "Returns the ID of the newly created Bank if successful");
            s.Response(400, "Returned if validation fails (missing fields or duplicate code)");
            s.Response(500, "Server error if an unexpected exception occurs");
        });

        Options(o => o.WithTags(EndpointGroups.Banks));
    }

    public override async Task HandleAsync(CreateBankRequest dto, CancellationToken ct)
    {
        var command = dto.Adapt<CreateBankCommand>();
        var result = await _mediator.Send(command, ct);
        await SendAsync(result.Value, cancellation: ct);
    }
}