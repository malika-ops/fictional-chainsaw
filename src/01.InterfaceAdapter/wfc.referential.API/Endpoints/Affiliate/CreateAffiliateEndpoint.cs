using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.Affiliates.Commands.CreateAffiliate;
using wfc.referential.Application.Affiliates.Dtos;

namespace wfc.referential.API.Endpoints.Affiliate;

public class CreateAffiliateEndpoint(IMediator _mediator) : Endpoint<CreateAffiliateRequest, Guid>
{
    public override void Configure()
    {
        Post("/api/affiliates");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Create a new Affiliate";
            s.Description = "Creates an affiliate with code, name, country, and other relevant information.";

            s.Response<Guid>(200, "Returns the ID of the newly created Affiliate if successful");
            s.Response(400, "Returned if validation fails (missing fields or invalid data)");
            s.Response(409, "Affiliate code already exists");
            s.Response(500, "Server error if an unexpected exception occurs");
        });

        Options(o => o.WithTags(EndpointGroups.Affiliates));
    }

    public override async Task HandleAsync(CreateAffiliateRequest req, CancellationToken ct)
    {
        var command = req.Adapt<CreateAffiliateCommand>();
        var result = await _mediator.Send(command, ct);
        await SendAsync(result.Value, cancellation: ct);
    }
}