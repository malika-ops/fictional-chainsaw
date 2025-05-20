using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.PartnerAccounts.Commands.CreatePartnerAccount;
using wfc.referential.Application.PartnerAccounts.Dtos;

namespace wfc.referential.API.Endpoints.PartnerAccount;

public class CreatePartnerAccount(IMediator _mediator) : Endpoint<CreatePartnerAccountRequest, Guid>
{
    public override void Configure()
    {
        Post("/api/partner-accounts");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Create a new Partner Account";
            s.Description = "Creates a partner account with account number, RIB, domiciliation, business name, short name, account balance, bank ID and account type.";

            s.Response<Guid>(200, "Returns the ID of the newly created Partner Account if successful");
            s.Response(400, "Returned if validation fails (missing fields or duplicate account number/RIB)");
            s.Response(500, "Server error if an unexpected exception occurs");
        });

        Options(o => o.WithTags(EndpointGroups.PartnerAccounts));
    }

    public override async Task HandleAsync(CreatePartnerAccountRequest dto, CancellationToken ct)
    {
        var command = dto.Adapt<CreatePartnerAccountCommand>();
        var result = await _mediator.Send(command, ct);
        await SendAsync(result.Value, cancellation: ct);
    }
}
