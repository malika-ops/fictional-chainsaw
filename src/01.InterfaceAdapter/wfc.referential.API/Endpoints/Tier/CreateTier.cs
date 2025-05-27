using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.Tiers.Commands.CreateTier;
using wfc.referential.Application.Tiers.Dtos;

namespace wfc.referential.API.Endpoints.Tier;

public class CreateTier(IMediator _mediator)
    : Endpoint<CreateTierRequest, Guid>
{
    public override void Configure()
    {
        Post("/api/tiers");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Create a new Tier";
            s.Description = "Creates a Tier with Name and Description. Name must be unique.";
            s.Response<Guid>(200, "Returns the TierId of the newly created row");
            s.Response(400, "Validation or business rule failed");
            s.Response(409, "Conflict with an existing Tier");
        });
        Options(o => o.WithTags(EndpointGroups.Tiers));
    }

    public override async Task HandleAsync(CreateTierRequest dto,
                                           CancellationToken ct)
    {
        var cmd = dto.Adapt<CreateTierCommand>();
        var result = await _mediator.Send(cmd, ct);
        await SendAsync(result.Value, cancellation: ct);
    }
}