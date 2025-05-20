using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.SupportAccounts.Commands.CreateSupportAccount;
using wfc.referential.Application.SupportAccounts.Dtos;

namespace wfc.referential.API.Endpoints.SupportAccount;

public class CreateSupportAccount(IMediator _mediator) : Endpoint<CreateSupportAccountRequest, Guid>
{
    public override void Configure()
    {
        Post("/api/support-accounts");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Create a new Support Account";
            s.Description = "Creates a support account with code, name, threshold, account balance, accounting number, partner ID, and support account type.";

            s.Response<Guid>(200, "Returns the ID of the newly created Support Account if successful");
            s.Response(400, "Returned if validation fails (missing fields or duplicate code/accounting number)");
            s.Response(500, "Server error if an unexpected exception occurs");
        });

        Options(o => o.WithTags(EndpointGroups.SupportAccounts));
    }

    public override async Task HandleAsync(CreateSupportAccountRequest dto, CancellationToken ct)
    {
        var command = dto.Adapt<CreateSupportAccountCommand>();
        var result = await _mediator.Send(command, ct);
        await SendAsync(result.Value, cancellation: ct);
    }
}