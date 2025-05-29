using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.Partners.Commands.CreatePartner;
using wfc.referential.Application.Partners.Dtos;

namespace wfc.referential.API.Endpoints.Partner;

public class CreatePartnerEndpoint(IMediator _mediator) : Endpoint<CreatePartnerRequest, Guid>
{
    public override void Configure()
    {
        Post("/api/partners");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Create a new Partner";
            s.Description = "Creates a partner with code, name, person type, and other relevant information.";

            s.Response<Guid>(200, "Returns the ID of the newly created Partner if successful");
            s.Response(400, "Returned if validation fails (missing fields or duplicate code)");
            s.Response(409, "Partner code already exists");
            s.Response(500, "Server error if an unexpected exception occurs");
        });

        Options(o => o.WithTags(EndpointGroups.Partners));
    }

    public override async Task HandleAsync(CreatePartnerRequest req, CancellationToken ct)
    {
        var command = req.Adapt<CreatePartnerCommand>();
        var result = await _mediator.Send(command, ct);
        await SendAsync(result.Value, cancellation: ct);
    }
}