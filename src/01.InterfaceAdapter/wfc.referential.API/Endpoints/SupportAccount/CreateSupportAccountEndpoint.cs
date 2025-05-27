using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.SupportAccounts.Commands.CreateSupportAccount;
using wfc.referential.Application.SupportAccounts.Dtos;

namespace wfc.referential.API.Endpoints.SupportAccount;

public class CreateSupportAccountEndpoint(IMediator _mediator) : Endpoint<CreateSupportAccountRequest, Guid>
{
    public override void Configure()
    {
        Post("/api/support-accounts");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Create a new Support Account";
            s.Description = "Creates a support account with the provided code, description, and other details.";
            s.Response<Guid>(200, "Identifier of the newly created support account");
            s.Response(400, "Validation failed");
            s.Response(409, "Support account code already exists");
            s.Response(500, "Unexpected server error");
        });
        Options(o => o.WithTags(EndpointGroups.SupportAccounts));
    }

    public override async Task HandleAsync(CreateSupportAccountRequest req, CancellationToken ct)
    {
        var command = req.Adapt<CreateSupportAccountCommand>();
        var result = await _mediator.Send(command, ct);
        await SendAsync(result.Value, cancellation: ct);
    }
}