using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.Currencies.Commands.CreateCurrency;
using wfc.referential.Application.Currencies.Dtos;

namespace wfc.referential.API.Endpoints.Currency;

public class CreateCurrencyEndpoint(IMediator _mediator) : Endpoint<CreateCurrencyRequest, Guid>
{
    public override void Configure()
    {
        Post("/api/currencies");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Create a new Currency";
            s.Description = "Creates a currency with the provided code, name, Arabic/English codes, and ISO code.";
            s.Response<Guid>(200, "Identifier of the newly created currency");
            s.Response(400, "Validation failed");
            s.Response(500, "Unexpected server error");
        });
        Options(o => o.WithTags(EndpointGroups.Currencies));
    }

    public override async Task HandleAsync(CreateCurrencyRequest req, CancellationToken ct)
    {
        var command = req.Adapt<CreateCurrencyCommand>();
        var result = await _mediator.Send(command, ct);
        await SendAsync(result.Value, cancellation: ct);
    }
}
