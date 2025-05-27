using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.Banks.Commands.CreateBank;
using wfc.referential.Application.Banks.Dtos;

namespace wfc.referential.API.Endpoints.Bank;

public class CreateBankEndpoint(IMediator _mediator) : Endpoint<CreateBankRequest, Guid>
{
    public override void Configure()
    {
        Post("/api/banks");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Create a new Bank";
            s.Description = "Creates a bank with the provided code, name, and abbreviation.";
            s.Response<Guid>(200, "Identifier of the newly created bank");
            s.Response(400, "Validation failed");
            s.Response(409, "Bank code already exists");
            s.Response(500, "Unexpected server error");
        });
        Options(o => o.WithTags(EndpointGroups.Banks));
    }

    public override async Task HandleAsync(CreateBankRequest req, CancellationToken ct)
    {
        var command = req.Adapt<CreateBankCommand>();
        var result = await _mediator.Send(command, ct);
        await SendAsync(result.Value, cancellation: ct);
    }
}