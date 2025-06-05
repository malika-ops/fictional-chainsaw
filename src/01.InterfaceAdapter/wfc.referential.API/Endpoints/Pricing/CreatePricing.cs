using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.Pricings.Commands.CreatePricing;
using wfc.referential.Application.Pricings.Dtos;

namespace wfc.referential.API.Endpoints.Pricing;

public class CreatePricing(IMediator _mediator)
    : Endpoint<CreatePricingRequest, Guid>
{
    public override void Configure()
    {
        Post("/api/pricings");
        AllowAnonymous();

        Summary(s =>
        {
            s.Summary = "Create a new Pricing line";
            s.Description = """
                            Adds a tariff line (pricing) for a given service & corridor.
                            Either FixedAmount or Rate must be supplied.
                            """;
            s.Response<Guid>(200, "ID of the newly-created Pricing");
            s.Response(400, "Validation errors");
            s.Response(409, "Conflict, duplicate pricing code");
            s.Response(500, "Unexpected server error");
        });

        Options(o => o.WithTags(EndpointGroups.Pricings));
    }

    public override async Task HandleAsync(CreatePricingRequest dto, CancellationToken ct)
    {
        var cmd = dto.Adapt<CreatePricingCommand>();
        var result = await _mediator.Send(cmd, ct);
        await SendAsync(result.Value, cancellation: ct);
    }
}