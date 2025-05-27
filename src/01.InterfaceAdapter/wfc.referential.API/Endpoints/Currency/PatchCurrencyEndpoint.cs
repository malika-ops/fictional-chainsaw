using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.Currencies.Commands.PatchCurrency;
using wfc.referential.Application.Currencies.Dtos;

namespace wfc.referential.API.Endpoints.Currency;

public class PatchCurrencyEndpoint(IMediator _mediator)
    : Endpoint<PatchCurrencyRequest, bool>
{
    public override void Configure()
    {
        Patch("/api/currencies/{CurrencyId}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Partially update a Currency";
            s.Description =
                "Updates only the supplied fields for the currency identified by {CurrencyId}.";
            s.Params["CurrencyId"] = "Currency GUID from route";

            s.Response<bool>(200, "Returns true if update succeeded");
            s.Response(400, "Validation / business rule failure");
            s.Response(404, "Currency not found");
            s.Response(409, "Conflict with an existing Currency");
        });
        Options(o => o.WithTags(EndpointGroups.Currencies));
    }

    public override async Task HandleAsync(PatchCurrencyRequest req, CancellationToken ct)
    {
        var cmd = req.Adapt<PatchCurrencyCommand>();
        var result = await _mediator.Send(cmd, ct);
        await SendAsync(result.Value, cancellation: ct);
    }
}
