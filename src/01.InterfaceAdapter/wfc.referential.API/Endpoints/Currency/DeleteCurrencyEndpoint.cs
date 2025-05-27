using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.Currencies.Commands.DeleteCurrency;
using wfc.referential.Application.Currencies.Dtos;

namespace wfc.referential.API.Endpoints.Currency;

public class DeleteCurrencyEndpoint(IMediator _mediator)
    : Endpoint<DeleteCurrencyRequest, bool>
{
    public override void Configure()
    {
        Delete("/api/currencies/{CurrencyId}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Delete a currency by GUID";
            s.Description = "Soft-deletes the currency identified by {CurrencyId}.";
            s.Params["CurrencyId"] = "GUID of the currency to delete";
            s.Response<bool>(200, "True if deletion succeeded");
            s.Response(400, "Validation / business rule failure");
        });
        Options(o => o.WithTags(EndpointGroups.Currencies));
    }

    public override async Task HandleAsync(DeleteCurrencyRequest req, CancellationToken ct)
    {
        var command = req.Adapt<DeleteCurrencyCommand>();
        var result = await _mediator.Send(command, ct);
        await SendAsync(result.Value, cancellation: ct);
    }
}