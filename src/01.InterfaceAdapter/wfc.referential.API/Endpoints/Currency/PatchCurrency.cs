using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.Currencies.Commands.PatchCurrency;
using wfc.referential.Application.Currencies.Dtos;

namespace wfc.referential.API.Endpoints.Currency;

public class PatchCurrency(IMediator _mediator) : Endpoint<PatchCurrencyRequest, Guid>
{
    public override void Configure()
    {
        Patch("/api/currencies/{CurrencyId}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Partially update a Currency's properties";
            s.Description = "Updates only the provided fields (code, name, or status) of the specified currency ID.";
            s.Params["CurrencyId"] = "Currency ID (GUID) from route";
            s.Response<Guid>(200, "The ID of the updated currency");
            s.Response(400, "If validation fails or the ID is invalid");
            s.Response(404, "Currency not found");
        });
        Options(o => o.WithTags(EndpointGroups.Currencies));
    }

    public override async Task HandleAsync(PatchCurrencyRequest req, CancellationToken ct)
    {
        var command = req.Adapt<PatchCurrencyCommand>();
        var result = await _mediator.Send(command, ct);
        await SendAsync(result.Value, cancellation: ct);
    }
}