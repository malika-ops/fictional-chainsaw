using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.Currencies.Commands.UpdateCurrency;
using wfc.referential.Application.Currencies.Dtos;

namespace wfc.referential.API.Endpoints.Currency;

public class UpdateCurrencyEndpoint(IMediator _mediator)
    : Endpoint<UpdateCurrencyRequest, bool>
{
    public override void Configure()
    {
        Put("/api/currencies/{CurrencyId}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Update an existing Currency";
            s.Description = "Updates the currency identified by {CurrencyId} with supplied body fields.";
            s.Params["CurrencyId"] = "Currency GUID (from route)";

            s.Response<bool>(200, "Returns true if update succeeded");
            s.Response(400, "Validation or business rule failure");
            s.Response(500, "Unexpected server error");
            s.Response(409, "Conflict with an existing Currency");
        });
        Options(o => o.WithTags(EndpointGroups.Currencies));
    }

    public override async Task HandleAsync(UpdateCurrencyRequest req, CancellationToken ct)
    {
        var cmd = req.Adapt<UpdateCurrencyCommand>();
        var result = await _mediator.Send(cmd, ct);
        await SendAsync(result.Value, cancellation: ct);
    }
}