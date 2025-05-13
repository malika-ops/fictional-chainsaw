using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.Currencies.Commands.UpdateCurrency;
using wfc.referential.Application.Currencies.Dtos;

namespace wfc.referential.API.Endpoints.Currency;

public class UpdateCurrency(IMediator _mediator) : Endpoint<UpdateCurrencyRequest, Guid>
{
    public override void Configure()
    {
        Put("/api/currencies/{CurrencyId}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Update an existing Currency";
            s.Description = "Updates the currency identified by CurrencyId with new code, name, status.";
            s.Params["CurrencyId"] = "Currency ID (GUID) from route";
            s.Response<Guid>(200, "Returns the ID of the updated currency upon success");
            s.Response(400, "Returned if validation fails (missing fields, invalid ID, etc.)");
            s.Response(404, "If the currency doesn't exist");
            s.Response(500, "Server error if something unexpected occurs");
        });
        Options(o => o.WithTags(EndpointGroups.Currencies));
    }

    public override async Task HandleAsync(UpdateCurrencyRequest dto, CancellationToken ct)
    {
        var command = dto.Adapt<UpdateCurrencyCommand>();
        var result = await _mediator.Send(command, ct);
        await SendAsync(result.Value, cancellation: ct);
    }
}