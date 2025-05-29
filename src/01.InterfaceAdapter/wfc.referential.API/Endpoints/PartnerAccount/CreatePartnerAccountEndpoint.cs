using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.PartnerAccounts.Commands.CreatePartnerAccount;
using wfc.referential.Application.PartnerAccounts.Dtos;

namespace wfc.referential.API.Endpoints.PartnerAccount;

public class CreatePartnerAccountEndpoint(IMediator _mediator) : Endpoint<CreatePartnerAccountRequest, Guid>
{
    public override void Configure()
    {
        Post("/api/partner-accounts");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Create a new Partner Account";
            s.Description = "Creates a partner account with account number, RIB, domiciliation, business name, short name, account balance, bank ID and account type.";
            s.Response<Guid>(200, "Identifier of the newly created partner account");
            s.Response(400, "Validation failed");
            s.Response(409, "Account number or RIB already exists");
            s.Response(500, "Unexpected server error");
        });
        Options(o => o.WithTags(EndpointGroups.PartnerAccounts));
    }

    public override async Task HandleAsync(CreatePartnerAccountRequest req, CancellationToken ct)
    {
        var command = req.Adapt<CreatePartnerAccountCommand>();
        var result = await _mediator.Send(command, ct);
        await SendAsync(result.Value, cancellation: ct);
    }
}